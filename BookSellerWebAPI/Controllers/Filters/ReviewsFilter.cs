using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class ReviewsFilter : BaseFilter
    {
        public int BookId { get; set; }

        public ReviewOrder OrderBy { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReviewOrder
    {
        MostRecent,
        BestRating,
        WorstRating
    }
}
