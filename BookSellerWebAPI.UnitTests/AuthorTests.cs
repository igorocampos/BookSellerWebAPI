using BookSellerWebAPI.Controllers;
using BookSellerWebAPI.Controllers.Filters;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BookSellerWebAPI.UnitTests.MockerHelper;

namespace BookSellerWebAPI.UnitTests
{
    public class AuthorTests : BaseTests<Author>
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestListTotalPages(int limit)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestListTotalPages)}_{limit}", TOTAL_RECORDS);
            var controller = new AuthorsController(dbContext);
            var filter = new AuthorFilter { Limit = limit };

            // Act
            var response = await controller.List(filter);

            // Assert
            Assert.Equal(ExpectedTotalPages(limit, TOTAL_RECORDS), response.Value.TotalPages);
        }

        [Fact]
        public async Task TestListCount()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestListCount)}", TOTAL_RECORDS);
            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.List(new AuthorFilter());

            // Assert
            Assert.Equal(TOTAL_RECORDS, response.Value.Items.Count);
        }

        [Theory]
        [InlineData(nameof(Author.FirstName))]
        [InlineData(nameof(Author.LastName))]
        public async Task TestListFilter(string propertyName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestListFilter)}", TOTAL_RECORDS);
            var firstAuthor = dbContext.Author.First();
            firstAuthor.FirstName = "[TEST]fIrStNaMe[TEST]";
            firstAuthor.LastName = "[TEST]lAsTNaMe[TEST]";
            dbContext.SaveChanges();

            var filter = new AuthorFilter();
            typeof(AuthorFilter).GetProperty(propertyName).SetValue(filter, propertyName);

            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.List(filter);

            // Assert
            Assert.Single(response.Value.Items);
        }

        [Theory]
        [InlineData(AuthorOrder.FirstName)]
        [InlineData(AuthorOrder.LastName)]
        public async Task TestListOrderby(AuthorOrder orderBy)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestListOrderby)}", TOTAL_RECORDS);
            var prop = typeof(Author).GetProperty(orderBy.ToString());
            prop.SetValue(dbContext.Author.First(), HIGHEST_NAME);
            prop.SetValue(dbContext.Author.Last(), LOWEST_NAME);

            dbContext.SaveChanges();

            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.List(new AuthorFilter { OrderBy = orderBy });

            // Assert
            Assert.Equal(orderBy, response.Value.OrderedBy);
            Assert.Equal(LOWEST_NAME, prop.GetValue(response.Value.Items.First()));
            Assert.Equal(HIGHEST_NAME, prop.GetValue(response.Value.Items.Last()));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        public async Task TestGet(int id)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestGet)}", TOTAL_RECORDS);

            // Act & Assert
            await Get(id, new AuthorsController(dbContext));
        }

        [Theory]
        [InlineData("FirstName", "LastName", "Biography", nameof(CreatedAtActionResult))]
        [InlineData("FirstName", "LastName", null, nameof(CreatedAtActionResult))]
        [InlineData("FirstName", "LastName", "", nameof(CreatedAtActionResult))]
        [InlineData(null, "LastName", "Biography")]
        [InlineData("", "LastName", "Biography")]
        [InlineData("FirstName", null, "Biography")]
        [InlineData("FirstName", "", "Biography")]
        public async Task TestCreate(string firstName, string lastName, string biography, string expectedResultName = null)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestCreate)}");
            var author = new Author
            {
                FirstName = firstName,
                LastName = lastName,
                Biography = biography
            };

            // Act & Assert
            await Create(expectedResultName, new AuthorsController(dbContext), author);
        }

        [Theory]
        [InlineData(1, 1, nameof(NoContentResult))]
        [InlineData(1, 2, nameof(BadRequestObjectResult))]
        [InlineData(0, 0, nameof(NotFoundResult))]
        public async Task TestUpdate(int findingId, int modelId, string expectedResultName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestUpdate)}", TOTAL_RECORDS);
            var author = dbContext.Author.First();
            author.Id = modelId;
            author.FirstName = "ChangedName";

            // Act & Assert
            await Update(findingId, expectedResultName, new AuthorsController(dbContext), author);
        }

        [Theory]
        [InlineData(1, nameof(OkObjectResult))]
        [InlineData(0, nameof(NotFoundObjectResult))]
        public async Task TestDelete(int id, string expectedResultName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(AuthorTests)}_{nameof(TestDelete)}", TOTAL_RECORDS);

            // Act & Arrange
            await Delete(id, expectedResultName, new AuthorsController(dbContext));
        }
    }
}
