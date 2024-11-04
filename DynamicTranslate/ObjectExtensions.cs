using DynamicTranslate.Attribute;
using DynamicTranslate.DB;
using DynamicTranslate.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicTranslate
{
    public static class ObjectExtensions
    {
        static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
        internal static IServiceProvider ServiceProvider { get; set; }
        public static async Task<T> Translate<T>(this T obj, string targetLanguageCode, string sourceLanguageCode = "en", CancellationToken cancellationToken = default)
        {
            List<TranslateExchangeStructure> matched = new List<TranslateExchangeStructure>(20);
            PropertiesExtension.ReadWritePropertiesRecursive(obj, matched);

            Repository<OverrideTranslation> overrideTranslationDSet = ServiceProvider.GetService<Repository<OverrideTranslation>>();
            Repository<OverrideTranslationDetail> overrideTranslationDetailSet = ServiceProvider.GetService<Repository<OverrideTranslationDetail>>();

            var overrideMatched =
                matched
                .Where(x =>
                    x.Attribute.IsEntityTranslation ||
                    x.Attribute.IsKeyTranslation)
                .ToArray();

            if (overrideTranslationDSet != null)
            {
                if (overrideMatched.Length > 0)
                {
                    string[] overrideMatchedQueryArray = overrideMatched.Select(x => $"{x.Attribute.Entity}_{x.Attribute.Property}_{x.KeyValue}").ToArray();
                    var translationRecords =
                        await overrideTranslationDSet
                        .DbSet
                        .Where(x =>
                        x.LanguageCode == sourceLanguageCode &&
                        overrideMatchedQueryArray.Contains(x.Entity + "_" + x.Property + "_" + x.Key))
                        .Select(x => new
                        {
                            Master = x,
                            Details = overrideTranslationDetailSet.DbSet
                            .Where(y =>
                                y.OverrideTranslationId == x.Id &&
                                y.LanguageCode == targetLanguageCode)
                            .ToList()
                        })
                        .ToArrayAsync(cancellationToken);

                    foreach (var translationRecord in translationRecords)
                    {

                        var matchedRecord =
                            overrideMatched
                            .FirstOrDefault(x =>
                                x.Attribute
                                .IsMatched(
                                    translationRecord.Master.Entity,
                                    translationRecord.Master.Property) &&
                                x.KeyValue == translationRecord.Master.Key);

                        if (matchedRecord == null)
                            continue;

                        matchedRecord.RelatedOverrideTranslation = translationRecord.Master;

                        if (translationRecord.Master.Text == matchedRecord.Text)
                        {
                            if (translationRecord.Details.Count > 0)
                            {
                                matchedRecord.Attribute.DatabaseRecordStatus = TranslateDatabaseRecordStatus.Found;
                                matchedRecord.Translation = translationRecord.Details.FirstOrDefault().Translation;
                            }
                            else
                            {
                                matchedRecord.Attribute.DatabaseRecordStatus = TranslateDatabaseRecordStatus.TargetLanguageNotFound;
                            }
                        }
                        else
                        {
                            matchedRecord.Attribute.DatabaseRecordStatus = TranslateDatabaseRecordStatus.Changed;
                        }
                    }
                }
            }

            var promotedToTranslation = matched.Where(x => x.Attribute.DatabaseRecordStatus != TranslateDatabaseRecordStatus.Found).ToImmutableHashSet().ToImmutableArray();

            string[] translation;
            try
            {
                await SemaphoreSlim.WaitAsync();
                translation =
                    await ServiceProvider.GetRequiredService<ITranslateEngine>()
                    .TranslateAsync(promotedToTranslation.Select(x => x.Text).ToArray(), targetLanguageCode, sourceLanguageCode);
            }
            finally
            {
                SemaphoreSlim?.Release();
            }

            bool translationSuccess = false;

            for (var i = 0; i < promotedToTranslation.Length; i++)
            {
                if (promotedToTranslation[i].Text != translation[i])
                {
                    translationSuccess = true;
                }
                promotedToTranslation[i].Translation = translation[i];
            }
            try
            {
                if (overrideTranslationDSet != null && translationSuccess)
                {
                    overrideTranslationDetailSet.ClearEntities();

                    foreach (var item in matched)
                    {
                        switch (item.Attribute.DatabaseRecordStatus)
                        {
                            case TranslateDatabaseRecordStatus.NotFound:
                                overrideTranslationDSet.DbSet.Add(
                                    new OverrideTranslation
                                    {
                                        CreatedAt = DateTime.Now,
                                        LanguageCode = sourceLanguageCode,
                                        Text = item.Text,
                                        Entity = item.Attribute.Entity,
                                        Property = item.Attribute.Property,
                                        Key = item.KeyValue,
                                        OverrideTranslationDetails = new OverrideTranslationDetail[]
                                         {
                                     new OverrideTranslationDetail
                                     {
                                         LanguageCode = targetLanguageCode,
                                         Translation= item.Translation,
                                     }
                                         }
                                    });
                                break;
                            case TranslateDatabaseRecordStatus.Changed:

                                overrideTranslationDSet.DbSet.Remove(item.RelatedOverrideTranslation);
                                overrideTranslationDSet.DbSet.Add(
                                    new OverrideTranslation
                                    {
                                        CreatedAt = DateTime.Now,
                                        LanguageCode = sourceLanguageCode,
                                        Text = item.Text,
                                        Entity = item.Attribute.Entity,
                                        Property = item.Attribute.Property,
                                        Key = item.KeyValue,
                                        OverrideTranslationDetails = new OverrideTranslationDetail[]
                                         {
                                     new OverrideTranslationDetail
                                     {
                                         LanguageCode = targetLanguageCode,
                                         Translation= item.Translation,
                                     }
                                         }
                                    });
                                break;
                            case TranslateDatabaseRecordStatus.TargetLanguageNotFound:
                                overrideTranslationDetailSet.DbSet.AddRange(
                                    new OverrideTranslationDetail
                                    {
                                        LanguageCode = targetLanguageCode,
                                        Translation = item.Translation,
                                        OverrideTranslationId = item.RelatedOverrideTranslation.Id,
                                    });
                                break;

                            default:
                                break;
                        }
                    }

                    await overrideTranslationDSet.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            PropertiesExtension.ReadWritePropertiesRecursive(obj, writeValues: matched.Select(x => x.Translation).ToList());
            return obj;

        }
    }

    internal static class PropertiesExtension
    {
        public static void ReadWritePropertiesRecursive(object value, List<TranslateExchangeStructure> matched = null, List<string> writeValues = null)
        {
            if (value is null)
                return;

            if (value is IEnumerable collection)
            {
                foreach (var val in collection)
                {
                    ReadWritePropertiesRecursive(val, matched, writeValues);
                }
                return;
            }

            var type = value.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType == type)
                    continue;
                if (!property.CanRead)
                    continue;
                var val = property.GetValue(value);
                if (value is IEnumerable enumerable)
                {
                    ReadWritePropertiesRecursive(enumerable, matched, writeValues);
                }
                var attr = property.GetCustomAttribute<TranslateAttribute>();
                string KeyValue = null;

                if (attr != null && property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(val.ToString()))
                {
                    if (matched != null)
                    {
                        if (!string.IsNullOrWhiteSpace(attr.Key))
                        {
                            KeyValue = type.GetProperty(attr.Key)?.GetValue(value, null)?.ToString();
                        }

                        matched.Add(new TranslateExchangeStructure(attr, val.ToString(), KeyValue));
                    }
                    else
                    {
                        property.SetValue(value, writeValues[0]);
                        writeValues.RemoveAt(0);
                    }
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    ReadWritePropertiesRecursive(val, matched, writeValues);
                }
            }
        }
    }
}
