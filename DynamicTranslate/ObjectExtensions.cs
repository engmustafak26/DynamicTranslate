using DynamicTranslate.Attribute;
using DynamicTranslate.DB;
using DynamicTranslate.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTranslate
{
    public static class ObjectExtensions
    {
        internal static IServiceProvider ServiceProvider { get; set; }
        public static async Task<T> Translate<T>(this T obj, string targetLanguageCode, string sourceLanguageCode = "en")
        {
            List<TranslateExchangeStructure> matched = new List<TranslateExchangeStructure>(20);
            PropertiesExtension.ReadWritePropertiesRecursive(obj, matched);

            var overrideTranslationDSet = ServiceProvider.GetService<Repository<OverrideTranslation>>();
            var overrideTranslationDetailSet = ServiceProvider.GetService<Repository<OverrideTranslationDetail>>();

            var overrideMatched = matched.Where(x => x.Attribute.IsEntityTranslation || x.Attribute.IsKeyTranslation)
                                         .ToArray();

            if (overrideTranslationDSet != null)
            {
                if (overrideMatched.Length > 0)
                {
                    string[] overrideMatchedQueryArray = overrideMatched.Select(x => $"{x.Attribute.Entity}_{x.Attribute.Property}_{(x.Attribute.IsKeyTranslation ? x.Attribute.Key : x.KeyValue)}").ToArray();
                    var translationRecords = await overrideTranslationDSet.DbSet
                                                        .Where(x => x.LanguageCode == sourceLanguageCode &&
                                                                overrideMatchedQueryArray.Contains(x.Entity + "_" + x.Property + "_" + x.Key))
                                                        .Select(x => new
                                                        {
                                                            Master = x,
                                                            Details = overrideTranslationDetailSet.DbSet
                                                                                .Where(y => y.OverrideTranslationId == x.Id
                                                                                              && y.LanguageCode == targetLanguageCode)
                                                                                .ToList()

                                                        })
                                                        .ToArrayAsync();

                    foreach (var translationRecord in translationRecords)
                    {

                        var matchedRecord = overrideMatched.FirstOrDefault(x => x.Attribute.IsMatched(translationRecord.Master.Entity, translationRecord.Master.Property)
                                                                             && (x.Attribute.IsKeyTranslation ? x.Attribute.Key : x.KeyValue) == translationRecord.Master.Key);
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

            var promotedToTranslation = matched.Where(x => x.Attribute.DatabaseRecordStatus != TranslateDatabaseRecordStatus.Found).ToArray();

            var translation = await ServiceProvider.GetRequiredService<ITranslateEngine>()
                                        .TranslateAsync(promotedToTranslation.Select(x => x.Text).ToArray(), targetLanguageCode, sourceLanguageCode);

            for (var i = 0; i < promotedToTranslation.Length; i++)
            {
                promotedToTranslation[i].Translation = translation[i];
            }

            if (overrideTranslationDSet != null)
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
                                    Key = item.Attribute.IsKeyTranslation ? item.Attribute.Key : item.KeyValue,
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
                                    Key = item.Attribute.IsKeyTranslation ? item.Attribute.Key : item.KeyValue,
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

            if (value is IEnumerable)
            {
                IList collection = (IList)value;
                foreach (var val in collection)
                    ReadWritePropertiesRecursive(val, matched, writeValues);
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
                var attr = property.GetCustomAttribute<TranslateAttribute>();
                string KeyValue = null;
                //if (property.CanWrite && val is T param)
                //{
                //    property.SetValue(value, func(param));
                //}
                if (attr != null && property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(val.ToString()))
                {
                    if (matched != null)
                    {
                        if (attr.IsEntityTranslation)
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
