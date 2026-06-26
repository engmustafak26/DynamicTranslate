namespace DynamicTranslate.Demo.Domain
{
    public class LookupDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long MasterId { get; set; }

        public LookupMaster  Master { get; set; }

    }

}
