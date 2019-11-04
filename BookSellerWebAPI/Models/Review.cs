using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellerWebAPI.Models
{
    [Table("reviews")]
    public class Review
    {
        [Column("book_id")]
        [ForeignKey("Reviews")]
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
