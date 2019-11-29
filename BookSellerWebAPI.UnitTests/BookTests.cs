using BookSellerWebAPI.Controllers;
using BookSellerWebAPI.Controllers.Filters;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BookSellerWebAPI.UnitTests.MockerHelper;

namespace BookSellerWebAPI.UnitTests
{
    public class BookTests : BaseTests<Book>
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestListTotalPages(int limit)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListTotalPages)}_{limit}", 1, TOTAL_RECORDS);
            var controller = new BooksController(dbContext);
            var filter = new BooksFilter { Limit = limit };

            // Act
            var response = await controller.List(filter);

            // Assert
            Assert.Equal(ExpectedTotalPages(limit, TOTAL_RECORDS), response.Value.TotalPages);
        }

        [Fact]
        public async Task TestListCount()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListCount)}", 1, TOTAL_RECORDS);
            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(new BooksFilter());

            // Assert
            Assert.Equal(TOTAL_RECORDS, response.Value.Items.Count);
        }

        [Fact]
        public async Task TestListAuthorNameFilter()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListAuthorNameFilter)}", TOTAL_RECORDS, TOTAL_RECORDS);
            var lastAuthor = dbContext.Author.Last();
            lastAuthor.FirstName = "fIrStNaMe";
            lastAuthor.LastName = "lAsTNaMe";

            dbContext.Book.First().Author = lastAuthor;
            dbContext.SaveChanges();

            var filter = new BooksFilter { AuthorName = "stname lastn" };
            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(filter);

            // Assert
            Assert.Single(response.Value.Items);

        }

        [Theory]
        [InlineData(10, null, 10, 10)]
        [InlineData(null, 10, 10, 1)]
        [InlineData(40, 60, 10, 3)]
        public async Task TestListPriceFilter(int? min, int? max, int totalRecords, int expectedResult)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListPriceFilter)}_{min?.ToString() ?? "null"}_{max?.ToString() ?? "null"}", 1, totalRecords);

            //Sets a price 10 times the book number (e.g.: The first book will cost 10.0, second book 20.0, the third 30.0, and so forth)
            int i = 1;
            foreach (var book in dbContext.Book)
                book.Price = i++ * 10;

            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(new BooksFilter { MinPrice = min, MaxPrice = max });

            // Assert
            Assert.Equal(expectedResult, response.Value.Items.Count);
        }

        [Fact]
        public async Task TestListOrderbyTitle()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListOrderbyTitle)}", 1, TOTAL_RECORDS);

            //Change the first book's title in order to be the last one when ordered by Title.
            dbContext.Book.First().Title = HIGHEST_NAME;
            dbContext.Book.Last().Title = LOWEST_NAME;
            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(new BooksFilter { OrderBy = BookOrder.Title });

            // Assert
            Assert.Equal(BookOrder.Title, response.Value.OrderedBy);
            Assert.Equal(LOWEST_NAME, response.Value.Items.First().Title);
            Assert.Equal(HIGHEST_NAME, response.Value.Items.Last().Title);
        }

        [Fact]
        public async Task TestListOrderbyAuthorName()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListOrderbyAuthorName)}", 3, TOTAL_RECORDS);

            //All books come from Seed() with the first author setted, so we'll alter the second and third authors' name to test it in the first and last book
            var firstAuthor = dbContext.Author.Skip(1).Take(1).First();
            var lastAuthor = dbContext.Author.Last();
            var firstBook = dbContext.Book.First();
            var lastBook = dbContext.Book.Last();

            firstAuthor.FirstName = HIGHEST_NAME;
            lastAuthor.FirstName = LOWEST_NAME;

            firstBook.Author = firstAuthor;
            lastBook.Author = lastAuthor;
            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(new BooksFilter { OrderBy = BookOrder.AuthorName });

            // Assert
            Assert.Equal(BookOrder.AuthorName, response.Value.OrderedBy);
            Assert.Equal(lastBook.Id, response.Value.Items.First().Id);
            Assert.Equal(firstBook.Id, response.Value.Items.Last().Id);
        }

        [Theory]
        [InlineData(0.5, 2.5, BookOrder.LowestPrice)]
        [InlineData(2.5, 0.5, BookOrder.HighestPrice)]
        public async Task TestListOrderbyPrice(decimal firstShownPrice, decimal lastShownPrice, BookOrder bookOrder)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestListOrderbyPrice)}_{bookOrder}", 1, TOTAL_RECORDS);

            //All books come from Seed() with price between $1 and $2
            var firstBook = dbContext.Book.First();
            var lastBook = dbContext.Book.Last();

            firstBook.Price = lastShownPrice;
            lastBook.Price = firstShownPrice;
            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.List(new BooksFilter { OrderBy = bookOrder });

            // Assert
            Assert.Equal(bookOrder, response.Value.OrderedBy);
            Assert.Equal(lastBook.Id, response.Value.Items.First().Id);
            Assert.Equal(firstBook.Id, response.Value.Items.Last().Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        public async Task TestGet(int id)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestGet)}", 1, TOTAL_RECORDS);

            // Act & Assert
            await Get(id, new BooksController(dbContext));
        }

        [Theory]
        [InlineData("Title", 0.1, nameof(CreatedAtActionResult))]
        [InlineData("", 0.1)]
        [InlineData(null, 0.1)]
        [InlineData("Title", null)]
        [InlineData("Title", 0d)]
        public async Task TestCreate(string title, double? price, string expectedResultName = null)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestCreate)}", 1);
            var book = new Book
            {
                Author = dbContext.Author.First(),
                Title = title,
            };

            //This simulates when user don't fill price field in the JSON that is sent
            if (price != null)
                book.Price = Convert.ToDecimal(price);

            // Act & Assert
            await Create(expectedResultName, new BooksController(dbContext), book);
        }

        [Theory]
        [InlineData(1, 1, nameof(NoContentResult))]
        [InlineData(1, 2, nameof(BadRequestObjectResult))]
        [InlineData(0, 0, nameof(NotFoundResult))]
        public async Task TestUpdate(int findingId, int modelId, string expectedResultName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestUpdate)}", 1, TOTAL_RECORDS);
            var book = dbContext.Book.First();
            book.Id = modelId;
            book.Title = "ChangedName";

            // Act & Assert
            await Update(findingId, expectedResultName, new BooksController(dbContext), book);
        }

        [Theory]
        [InlineData(1, nameof(OkObjectResult))]
        [InlineData(0, nameof(NotFoundObjectResult))]
        public async Task TestDelete(int id, string expectedResultName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(BookTests)}_{nameof(TestDelete)}", 1, TOTAL_RECORDS);

            // Act & Assert
            await Delete(id, expectedResultName, new BooksController(dbContext));
        }
    }
}
