using DynamicTranslate.Attribute;
using DynamicTranslate.DB;

namespace DynamicTranslate
{
    internal class TranslateExchangeStructure
    {
        public TranslateExchangeStructure(TranslateAttribute attribute, string text, string keyValue)
        {
            Attribute = attribute;
            Text = text;
            KeyValue = keyValue;
        }

        public TranslateAttribute Attribute { get; set; }
        public string Text { get; set; }
        public string KeyValue { get; set; }

        public string Translation { get; set; }
        public OverrideTranslation RelatedOverrideTranslation { get; set; }
    }
}
