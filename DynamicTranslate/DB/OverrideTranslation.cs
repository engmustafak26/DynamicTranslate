using System;
using System.Collections.Generic;

namespace DynamicTranslate.DB
{
    public class OverrideTranslation
    {
        public OverrideTranslation()
        {
            OverrideTranslationDetails = new HashSet<OverrideTranslationDetail>();
        }
        public long Id { get; set; }
        public string LanguageCode { get; set; }
        public string Entity { get; set; }
        public string Property { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<OverrideTranslationDetail> OverrideTranslationDetails { get; set; }
    }
}
