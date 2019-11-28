using BookSellerWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BookSellerWebAPI.Data
{
    public static class DatabaseHelper
    {
        public static IHost CreateDatabase<T>(this IHost webHost) where T : DbContext
        {
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;

            // According to https://github.com/peter-evans/docker-compose-healthcheck/issues/3#issuecomment-329037485
            // docker-compose stopped accepting a health condition in `depends_on` from v3 and above.
            // we are making a resilient application here, trying to reconect 3 times with 5 seconds interval
            Exception lastException = null;
            for (int count = 0; count < 3; count++)
            {
                try
                {
                    var db = services.GetRequiredService<T>();
                    db.Database.Migrate();
                    return webHost;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                Thread.Sleep(5000);
            }

            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(lastException, "Database Creation/Migrations failed!");

            return webHost;
        }

        public static bool Exists<T>(this DbSet<T> set, long id) where T : BaseModel
           => set.Any(e => e.Id == id);

        //Credit: https://stackoverflow.com/a/33833209
        public static bool IsDefault(this object obj, Type runtimeType)
        {
            if (obj == null)
                return true;

            if (runtimeType == null)
                throw new ArgumentNullException(nameof(runtimeType));

            // Handle non-null reference types.
            if (!runtimeType.IsValueType)
                return false;

            // Nullable, but not null
            if (Nullable.GetUnderlyingType(runtimeType) != null)
                return false;

            // Use CreateInstance as the most reliable way to get default value for a value type
            object defaultValue = Activator.CreateInstance(runtimeType);

            return defaultValue.Equals(obj);
        }

        private static Random random = new Random();
        public static BookSellerContext Seed(this BookSellerContext dbContext, int authorCount = 0, int bookCount = 0, int reviewCount = 0)
        {
            //Load context with N Authors with random values
            for (int i = 1; i <= authorCount; i++)
                dbContext.Author.Add(new Author
                {
                    FirstName = RandomStrings(1).First(),
                    LastName = RandomStrings(1).First()
                });

            dbContext.SaveChanges();

            //Load context with N Books of the first author
            for (int i = 1; i <= bookCount; i++)
                dbContext.Book.Add(new Book
                {
                    Author = dbContext.Author.FirstOrDefault() ?? throw new Exception("Can't seed Books without any Author."),
                    Price = Math.Round(Convert.ToDecimal(random.NextDouble()), 2) + 1,
                    Title = string.Join(' ', RandomStrings(3)),
                });

            dbContext.SaveChanges();

            //Load context with N Reviews in the first book
            for (int i = 1; i <= reviewCount; i++)
                dbContext.Review.Add(new Review
                {
                    Book = dbContext.Book.FirstOrDefault() ?? throw new Exception("Can't seed Reviews without any Book."),
                    Comment = string.Join(' ', RandomStrings(10)),
                    Rating = random.Next(1, 5)
                });

            dbContext.SaveChanges();

            return dbContext;
        }

        public static IEnumerable<string> RandomStrings(int count)
        {
            for (int i = 0; i < count; i++)
            {
                //Generate a random 11 characters name
                yield return Path.GetRandomFileName();
            }
        }
    }
}
