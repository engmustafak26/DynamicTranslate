using DynamicTranslate.Attribute;

namespace DynamicTranslate.Demo.DTO
{
    public class LookupDetailDTO
    {
        public long Id { get; set; }

        [Translate(nameof(LookupDetailDTO), nameof(Name), nameof(Id))]
        public string Name { get; set; }


    }

}
