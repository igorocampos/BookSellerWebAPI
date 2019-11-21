using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class BooksFilter : BaseFilter
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public int? MinAverageRating { get; set; }
        public int? MaxAverageRating { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public BookOrder OrderBy { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookOrder
    {
        Title,
        AuthorName,
        BestAverageRating,
        WorstAverageRating,
        LowestPrice,
        HighestPrice
    }
}
