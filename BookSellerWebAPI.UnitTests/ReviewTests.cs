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
    public class ReviewTests : BaseTests<Review>
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestListTotalPages(int limit)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestListTotalPages)}_{limit}", 1, 1, TOTAL_RECORDS);
            var controller = new BooksController(dbContext);
            var filter = new ReviewsFilter { Limit = limit };

            // Act
            var response = await controller.ListReviews(1, filter);

            // Assert
            Assert.Equal(ExpectedTotalPages(limit, TOTAL_RECORDS), response.Value.TotalPages);
        }

        [Fact]
        public async Task TestListCount()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestListCount)}", 1, 1, TOTAL_RECORDS);
            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.ListReviews(1, new ReviewsFilter());

            // Assert
            Assert.Equal(TOTAL_RECORDS, response.Value.Items.Count);
        }

        [Theory]
        [InlineData(1, null, 5, 5)]
        [InlineData(null, 1, 5, 1)]
        [InlineData(2, 4, 5, 3)]
        public async Task TestListRatingFilter(int? min, int? max, int totalRecords, int expectedResult)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestListRatingFilter)}_{min?.ToString() ?? "null"}_{max?.ToString() ?? "null"}", 1, 1, totalRecords);

            int i = 1;
            foreach (var review in dbContext.Review)
                review.Rating = i++;

            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            var filter = new ReviewsFilter();

            if (min != null)
                filter.MinRating = min.Value;

            if (max != null)
                filter.MaxRating = max.Value;

            // Act
            var response = await controller.ListReviews(1, filter);

            // Assert
            Assert.Equal(expectedResult, response.Value.Items.Count);
        }

        [Theory]
        [InlineData(5, 1, ReviewOrder.BestRating)]
        [InlineData(1, 5, ReviewOrder.WorstRating)]
        public async Task TestListOrderbyRating(int firstShownRating, int lastShownRating, ReviewOrder reviewOrder)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestListOrderbyRating)}_{reviewOrder}", 1, 1, 2);

            var firstReview = dbContext.Review.First();
            var lastReview = dbContext.Review.Last();

            firstReview.Rating = lastShownRating;
            lastReview.Rating = firstShownRating;
            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.ListReviews(1, new ReviewsFilter { OrderBy = reviewOrder });

            // Assert
            Assert.Equal(reviewOrder, response.Value.OrderedBy);
            Assert.Equal(lastReview.Id, response.Value.Items.First().Id);
            Assert.Equal(firstReview.Id, response.Value.Items.Last().Id);
        }

        [Fact]
        public async Task TestListOrderbyMostRecent()
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestListOrderbyMostRecent)}", 1, 1, TOTAL_RECORDS);

            var firstReview = dbContext.Review.First();
            var lastReview = dbContext.Review.Last();

            int i = 0;
            foreach (var review in dbContext.Review)
                review.Creation = DateTime.Now.AddDays(i++);

            dbContext.SaveChanges();

            var controller = new BooksController(dbContext);

            // Act
            var response = await controller.ListReviews(1, new ReviewsFilter { OrderBy = ReviewOrder.MostRecent });

            // Assert
            Assert.Equal(ReviewOrder.MostRecent, response.Value.OrderedBy);
            Assert.Equal(lastReview.Id, response.Value.Items.First().Id);
            Assert.Equal(firstReview.Id, response.Value.Items.Last().Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        public async Task TestGet(int id)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestGet)}", 1, 1, TOTAL_RECORDS);

            // Act & Assert
            await Get(id, new ReviewsController(dbContext));
        }

        [Theory]
        [InlineData("Comment", 1, nameof(CreatedAtActionResult))]
        [InlineData("", 1, nameof(CreatedAtActionResult))]
        [InlineData(null, 1, nameof(CreatedAtActionResult))]
        [InlineData("Comment", 0)]
        [InlineData("Comment", null)]
        public async Task TestCreate(string comment, int? rating, string expectedResultName = null)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestCreate)}", 1, 1);
            var book = dbContext.Book.First();
            var review = new Review
            {
                Book = book,
                Comment = comment
            };

            //This simulates when user don't fill price field in the JSON that is sent
            if (rating != null)
                review.Rating = rating.Value;

            var controller = new BooksController(dbContext);

            // Act
            ActionResult<Review> response = null;

            // Mimic the behaviour of the model binder which is responsible for Validating the Model
            if (controller.Validate(review))
                response = await controller.Post(1, review);

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
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestUpdate)}", 1, 1, TOTAL_RECORDS);
            var review = dbContext.Review.First();
            review.Id = modelId;
            review.Rating = 1;

            //Act & Assert
            await Update(findingId, expectedResultName, new ReviewsController(dbContext), review);
        }

        [Theory]
        [InlineData(1, nameof(OkObjectResult))]
        [InlineData(0, nameof(NotFoundObjectResult))]
        public async Task TestDelete(int id, string expectedResultName)
        {
            // Arrange
            using var dbContext = GetBookSellerContext($"{nameof(ReviewTests)}_{nameof(TestDelete)}", 1, 1, TOTAL_RECORDS);

            //Act & Assert
            await Delete(id, expectedResultName, new ReviewsController(dbContext));
        }
    }
}
