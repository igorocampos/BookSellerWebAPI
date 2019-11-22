using System;
using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class BooksFilter : BaseFilter<BookOrder>
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public int? MinAverageRating { get; set; }
        public int? MaxAverageRating { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
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
