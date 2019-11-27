using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BookSellerWebAPI.Models
{
    public class BookSellerContext : DbContext
    {
        public BookSellerContext(DbContextOptions<BookSellerContext> options) : base(options) { }

        public DbSet<Book> Book { get; set; }

        public DbSet<Review> Review { get; set; }

        public DbSet<Author> Author { get; set; }

        public DbSet<IdentityUser> ApplicationUser { get; set; }
    }
}
