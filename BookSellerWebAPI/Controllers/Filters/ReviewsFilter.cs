namespace BookSellerWebAPI.Controllers.Filters
{
    public class ReviewsFilter : BaseFilter
    {
        public int BookId { get; set; }

        public ReviewOrder OrderBy { get; set; }
    }

    public enum ReviewOrder
    {
        MostRecent,
        BestRating,
        WorstRating
    }
}
