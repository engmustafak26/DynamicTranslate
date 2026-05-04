namespace DynamicTranslate.DB
{
    public class OverrideTranslationDetail
    {
        public long Id { get; set; }
        public string LanguageCode { get; set; }
        public long OverrideTranslationId { get; set; }
        public string Translation { get; set; }

        public OverrideTranslation OverrideTranslation { get; set; }
    }
}
