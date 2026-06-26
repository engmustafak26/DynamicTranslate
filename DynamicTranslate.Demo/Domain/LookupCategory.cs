namespace DynamicTranslate.Demo.Domain
{
    public class LookupCategory
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<LookupMaster> LookupMasters { get; set; }

    }

}
