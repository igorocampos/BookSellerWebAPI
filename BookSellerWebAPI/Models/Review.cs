using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookSellerWebAPI.Models
{
    [Table("reviews")]
    public class Review : BaseModel
    {
        [Column("book_id")]
        [JsonIgnore]
        public long BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; }

        [Range(1, 5)]
        [Column("rating")]
        public int Rating { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

        [Column("creation")]
        public DateTime Creation { get; set; }
    }
}
