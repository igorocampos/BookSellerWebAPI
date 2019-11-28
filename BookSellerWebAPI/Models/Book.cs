using BookSellerWebAPI.Data;
using BookSellerWebAPI.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Models
{
    [Table("books")]
    public class Book : BaseModel
    {
        [Required(ErrorMessage = "Book's title is not optional.", AllowEmptyStrings = false)]
        [Column("title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Book's author id is not optional.")]
        [Column("author_id")]
        public long AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; }

        [Column("average_rating")]
        [Editable(false)]
        public decimal AverageRating { get; set; }

        [Min(0.01, "Price must be at least 0.01")]
        [Column("price")]
        public decimal Price { get; set; }

        public override bool Validate(out string error, BookSellerContext context)
        {
            if (Author is Author author)
            {
                this.AuthorId = author.Id;
                this.Author = null;
            }

            if (!context.Author.Exists(this.AuthorId))
            {
                error = $"{nameof(AuthorId)} - `{this.AuthorId}` does not exist in the database.";
                return false;
            }
            return base.Validate(out error, context);
        }

        public override void IncludeChildren(BookSellerContext context)
            => context.Book.Include(book => book.Author).FirstOrDefaultAsync(book => book.Id == this.Id);
    }
}
