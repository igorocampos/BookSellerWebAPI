﻿using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        [Editable(false)]
        public DateTime? Creation { get; set; }

        public override void IncludeChildren(BookSellerContext context)
        {
            context.Review.Include(review => review.Book).FirstOrDefaultAsync(review => review.Id == this.Id);
            context.Book.Include(book => book.Author).FirstOrDefaultAsync(book => book.Id == this.BookId);
        }

        public void CalculateAverageRating(BookSellerContext context)
        {
            var book = context.Book.Find(this.BookId);
            book.AverageRating = Math.Round(Convert.ToDecimal(context.Review.Where(item => item.BookId == this.BookId).Average(item => item.Rating)), 1);
            context.SaveChanges();
        }
    }
}
