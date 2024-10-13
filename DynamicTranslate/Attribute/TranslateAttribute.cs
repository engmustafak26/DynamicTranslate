using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicTranslate.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]

    public class TranslateAttribute : System.Attribute
    {
        public TranslateAttribute()
        {

            DatabaseRecordStatus = TranslateDatabaseRecordStatus.NotApplicableToRecord;
        }

        public TranslateAttribute(string key)
        {
            Key = key;

            DatabaseRecordStatus = TranslateDatabaseRecordStatus.NotFound;
        }

        public TranslateAttribute(string entity, string property, string key)
        {
            Entity = entity;
            Property = property;
            Key = key;

            DatabaseRecordStatus = TranslateDatabaseRecordStatus.NotFound;
        }

        public string Entity { get; private set; }
        public string Property { get; private set; }
        public string Key { get; private set; }

        internal TranslateDatabaseRecordStatus DatabaseRecordStatus { get; set; }
        internal bool IsEntityTranslation => !string.IsNullOrWhiteSpace(Entity);
        internal bool IsKeyTranslation => !IsEntityTranslation && !string.IsNullOrWhiteSpace(Key);


        internal bool IsMatched(string entity, string property)
        {
            if (string.IsNullOrWhiteSpace(Entity) && !string.IsNullOrWhiteSpace(entity))
            {
                return false;
            }

            

            if (string.IsNullOrWhiteSpace(Entity))
            {
                return false;
            }

            return Entity == entity && Property == property ;
        }

        public void ChangeKey(string newKey)
        {
            Key = newKey;
        }
    }

    internal enum TranslateDatabaseRecordStatus
    {
        NotFound,
        Found,
        Changed,
        TargetLanguageNotFound,
        NotApplicableToRecord,
    }
}
