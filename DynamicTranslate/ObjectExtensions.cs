using DynamicTranslate.Attribute;
using DynamicTranslate.DB;
using DynamicTranslate.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            var dbContext = ServiceProvider.GetService<TranslationDbContext>();

            var overrideMatched = matched.Where(x => x.Attribute.IsEntityTranslation || x.Attribute.IsKeyTranslation)
                                         .ToArray();

            if (dbContext != null)
            {
                if (overrideMatched.Length > 0)
                {
                    string[] overrideMatchedQueryArray = overrideMatched.Select(x => $"{x.Attribute.Entity}_{x.Attribute.Property}_{(x.Attribute.IsKeyTranslation ? x.Attribute.Key : x.KeyValue)}").ToArray();
                    var translationRecords = await dbContext.OverrideTranslations
                                                        .Include(x => x.OverrideTranslationDetails
                                                                        .Where(y => y.LanguageCode == targetLanguageCode))
                                                        .Where(x => x.LanguageCode == sourceLanguageCode &&
                                                                overrideMatchedQueryArray.Contains(x.Entity + "_" + x.Property + "_" + x.Key))
                                                        .ToArrayAsync();

                    foreach (var translationRecord in translationRecords)
                    {
                        translationRecord.OverrideTranslationDetails = translationRecord.OverrideTranslationDetails
                                                                                    .Where(x => x.LanguageCode == targetLanguageCode)
                                                                                    .ToArray();
                        var matchedRecord = overrideMatched.FirstOrDefault(x => x.Attribute.IsMatched(translationRecord.Entity, translationRecord.Property)
                                                                             && (x.Attribute.IsKeyTranslation ? x.Attribute.Key : x.KeyValue) == translationRecord.Key);
                        if (matchedRecord == null)
                            continue;

                        matchedRecord.RelatedOverrideTranslation = translationRecord;

                        if (translationRecord.Text == matchedRecord.Text)
                        {
                            if (translationRecord.OverrideTranslationDetails.Count > 0)
                            {
                                matchedRecord.Attribute.DatabaseRecordStatus = TranslateDatabaseRecordStatus.Found;
                                matchedRecord.Translation = translationRecord.OverrideTranslationDetails.FirstOrDefault().Translation;
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

            if (dbContext != null)
            {
                foreach (var item in matched)
                {
                    switch (item.Attribute.DatabaseRecordStatus)
                    {
                        case TranslateDatabaseRecordStatus.NotFound:
                            dbContext.OverrideTranslations.Add(
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

                            dbContext.OverrideTranslations.Remove(item.RelatedOverrideTranslation);
                            dbContext.OverrideTranslations.Add(
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
                            dbContext.OverrideTranslationDetails.Add(
                                new OverrideTranslationDetail
                                {
                                    LanguageCode = targetLanguageCode,
                                    Translation = item.Translation,
                                    OverrideTranslation = item.RelatedOverrideTranslation,
                                });
                            break;

                        default:
                            break;
                    }
                }
                await dbContext.SaveChangesAsync();
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
                if ( attr != null && property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(val.ToString()))
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
