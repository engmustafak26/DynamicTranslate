namespace DynamicTranslate.Demo.Domain
{
    public class LookupMaster
    {
        public long Id { get; set; }    
        public string Name { get; set; } 
        
        public long CategoryId { get; set; }
        public LookupCategory Category { get; set; }

        public ICollection<LookupDetail> LookupDetails { get; set; }

    }

}
