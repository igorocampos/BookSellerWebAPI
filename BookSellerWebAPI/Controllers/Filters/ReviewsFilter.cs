using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class ReviewsFilter : BaseFilter<ReviewOrder>
    {
        public int MinRating { get; set; }
        public int MaxRating { get; set; } = 5;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReviewOrder
    {
        MostRecent,
        BestRating,
        WorstRating
    }
}
