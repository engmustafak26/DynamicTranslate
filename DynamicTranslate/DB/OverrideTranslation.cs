using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;

namespace DynamicTranslate.DB
{
    public class OverrideTranslation
    {
        public long Id { get; set; }
        public string LanguageCode { get; set; }
        public string Entity { get;  set; }
        public string Property { get;  set; }
        public string Key { get;  set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<OverrideTranslationDetail> OverrideTranslationDetails { get; set; }
    }

    public class OverrideTranslationDetail
    {
        public long Id { get; set; }
        public string LanguageCode { get; set; }        
        public long OverrideTranslationId { get; set; }
        public string Translation { get; set; }

        public OverrideTranslation OverrideTranslation { get; set; }    
    }
}
