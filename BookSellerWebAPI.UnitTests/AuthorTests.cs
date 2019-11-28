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
    public class AuthorTests
    {
        const int TOTAL_RECORDS = 5;

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestListTotalPages(int limit)
        {
            using (var dbContext = GetBookSellerContext($"{nameof(TestListTotalPages)}_{limit}", TOTAL_RECORDS))
            {
                // Arrange
                var controller = new AuthorsController(dbContext);
                var filter = new AuthorFilter { Limit = limit };

                // Act
                var response = await controller.List(filter);

                // Assert
                Assert.Equal(ExpectedTotalPages(limit, TOTAL_RECORDS), response.Value.TotalPages);
            }
        }

        [Fact]
        public async Task TestListCount()
        {
            using var dbContext = GetBookSellerContext(nameof(TestListCount), TOTAL_RECORDS);
            // Arrange
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
            using (var dbContext = GetBookSellerContext(nameof(TestListFilter), TOTAL_RECORDS))
            {
                // Arrange
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
        }

        [Theory]
        [InlineData(AuthorOrder.FirstName)]
        [InlineData(AuthorOrder.LastName)]
        public async Task TestListOrderby(AuthorOrder orderBy)
        {
            const string LOWEST = "'''";
            const string HIGHEST = "ZZZ";
            using (var dbContext = GetBookSellerContext(nameof(TestListOrderby), TOTAL_RECORDS))
            {
                // Arrange

                var prop = typeof(Author).GetProperty(orderBy.ToString());
                prop.SetValue(dbContext.Author.First(), HIGHEST);
                prop.SetValue(dbContext.Author.Last(), LOWEST);

                dbContext.SaveChanges();

                var controller = new AuthorsController(dbContext);

                // Act
                var response = await controller.List(new AuthorFilter { OrderBy = orderBy });

                // Assert
                Assert.Equal(orderBy, response.Value.OrderedBy);
                Assert.Equal(LOWEST, prop.GetValue(response.Value.Items.First()));
                Assert.Equal(HIGHEST, prop.GetValue(response.Value.Items.Last()));
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        public async Task TestGet(int id)
        {
            using var dbContext = GetBookSellerContext(nameof(TestGet), TOTAL_RECORDS);

            // Arrange
            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.Get(id);

            // Assert
            Assert.True(id <= 0 ? response.Result is NotFoundObjectResult : response.Value.Id == id, "Either the returned model ID was different from the requested, or it found a model when it should not.");
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
            using var dbContext = GetBookSellerContext(nameof(TestCreate));

            // Arrange
            var author = new Author
            {
                FirstName = firstName,
                LastName = lastName,
                Biography = biography
            };

            var controller = new AuthorsController(dbContext);

            // Act
            ActionResult<Author> response = null;

            // Mimic the behaviour of the model binder which is responsible for Validating the Model
            if (controller.Validate(author))
                response = await controller.Post(author);

            // Assert
            Assert.True(response?.Result.GetType().Name == expectedResultName, expectedResultName is null
                                                                               ? "It was expected that this test case would not pass the model biding validation, however it did."
                                                                               : $"It was expected that this test case would have a '{expectedResultName}' return, however it did not.");
        }

        [Theory]
        [InlineData(1, 1, nameof(NoContentResult))]
        [InlineData(1, 2, nameof(BadRequestObjectResult))]
        [InlineData(0, 0, nameof(NotFoundResult))]
        public async Task TestUpdate(int findingId, int modelId, string expectedResultName)
        {
            const string NEW_NAME = "ChangedName";
            using var dbContext = GetBookSellerContext(nameof(TestUpdate), TOTAL_RECORDS);

            // Arrange
            var author = dbContext.Author.First();
            author.Id = modelId;
            author.FirstName = NEW_NAME;

            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.Put(findingId, author);

            // Assert
            Assert.True(response.GetType().Name == expectedResultName, $"It was expected that this test case would have a '{expectedResultName}' return, however it did not. Actual result type: {response.GetType()}.");
        }

        [Theory]
        [InlineData(1, nameof(OkObjectResult))]
        [InlineData(0, nameof(NotFoundObjectResult))]
        public async Task TestDelete(int id, string expectedResultName)
        {
            using var dbContext = GetBookSellerContext(nameof(TestDelete), TOTAL_RECORDS);

            // Arrange
            var controller = new AuthorsController(dbContext);

            // Act
            var response = await controller.Delete(id);

            // Assert
            Assert.True(response.Result.GetType().Name == expectedResultName, $"It was expected that this test case would have a '{expectedResultName}' return, however it did not. Actual result type: {response.Result.GetType()}.");
        }
    }
}
