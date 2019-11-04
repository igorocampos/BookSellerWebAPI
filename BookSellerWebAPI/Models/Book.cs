using BookSellerWebAPI.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellerWebAPI.Models
{
    [Table("books")]
    public class Book : BaseModel
    {
        [Required(ErrorMessage = "Book's title is not optional.", AllowEmptyStrings = false)]
        [Column("title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author's name is not optional.", AllowEmptyStrings = false)]
        [Column("author_name")]
        public string AuthorName { get; set; }

        [Column("average_rating")]
        public decimal AverageRating { get; set; }

        [Min("0.01", "Price must be at least 0.01")]
        [Column("price")]
        public decimal Price { get; set; }
    }
}
