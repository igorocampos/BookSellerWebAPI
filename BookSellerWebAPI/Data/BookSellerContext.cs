using Microsoft.EntityFrameworkCore;

namespace BookSellerWebAPI.Models
{
    public class BookSellerContext : DbContext
    {
        public BookSellerContext(DbContextOptions<BookSellerContext> options) : base(options) { }

        public DbSet<Book> Book { get; set; }

        public DbSet<Review> Review { get; set; }
    }
}
