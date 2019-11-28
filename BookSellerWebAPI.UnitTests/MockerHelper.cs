using BookSellerWebAPI.Data;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BookSellerWebAPI.UnitTests
{
    public static class MockerHelper
    {
        public static BookSellerContext GetBookSellerContext(string dbName, int authorCount = 0, int bookCount = 0, int reviewCount = 0)
        {
            // Create options for DbContext instance
            var options = new DbContextOptionsBuilder<BookSellerContext>()
                              .UseInMemoryDatabase(databaseName: dbName)
                              .Options;

            // Create instance of DbContext
            var dbContext = new BookSellerContext(options);

            // Add entities in memory
            return dbContext.Seed(authorCount, bookCount, reviewCount);
        }

        public static bool Validate(this ControllerBase controller, BaseModel model)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            foreach (var validationResult in validationResults)
                controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? "", validationResult.ErrorMessage);

            return controller.ModelState.IsValid;
        }

        public static int ExpectedTotalPages(int limit, int total)
        {
            if (limit <= 0)
                limit = 1;

            int remainder = total % limit;
            int division = total / limit;
            return remainder == 0 ? division : division + 1;
        }

    }
}
