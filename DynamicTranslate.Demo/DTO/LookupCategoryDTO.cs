using DynamicTranslate.Attribute;

namespace DynamicTranslate.Demo.DTO
{
    public class LookupCategoryDTO
    {
        public long Id { get; set; }

        [Translate(nameof(LookupCategoryDTO), nameof(Name), nameof(Id))]
        public string Name { get; set; }

        public LookupMasterDTO[] Lookups { get; set; }

    }

}
