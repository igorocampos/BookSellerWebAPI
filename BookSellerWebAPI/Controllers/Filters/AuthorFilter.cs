using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class AuthorFilter : BaseFilter<AuthorOrder>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuthorOrder
    {
        FirstName,
        LastName
    }
}
