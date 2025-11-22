using Common.Domain.Entities;

namespace SearchService.Domain.Entities
{
    public class SearchMetadata : BaseEntity
    {
        public string SearchVector { get; set; } 
        public int ViewCount { get; set; }
        public decimal Relevance { get; set; }
        public DateTimeOffset LastIndexed { get; set; }
        public DateTime IndexedAt { get; set; }
        public string Source { get; set; }
        public SearchItem SearchItem { get; set; }
        public Guid SearchItemId { get; set; }
    }
}