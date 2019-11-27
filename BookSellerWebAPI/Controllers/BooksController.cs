using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSellerWebAPI.Models;
using BookSellerWebAPI.Controllers.Filters;
using System;
using BookSellerWebAPI.Data;

namespace BookSellerWebAPI.Controllers
{
    public class BooksController : Controller<Book>
    {
        public BooksController(BookSellerContext context) : base(context)
            => this.dbSet = context.Book;

        /// <summary>
        /// Gets a paged list with Books found according to the search parameters.
        /// </summary>
        /// <remarks>
        /// If no page number is informed, it will return the first page. If no record limit (page size) is informed, it will be 100 records.
        /// </remarks>
        /// <response code="200"><pre>Returns a JSON of the page.</pre></response>
        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Book, BookOrder>>> List([FromQuery] BooksFilter filter)
        {
            var filteredData = context.Book.Include(book => book.Author).Where(book => (string.IsNullOrEmpty(filter.Title) || book.Title.ToUpper().Contains(filter.Title.ToUpper()))
                                                   && (string.IsNullOrEmpty(filter.AuthorName) || book.AuthorFullName.ToUpper().Contains(filter.AuthorName.ToUpper()))
                                                   && (filter.MinAverageRating == null || book.AverageRating >= filter.MinAverageRating)
                                                   && (filter.MaxAverageRating == null || book.AverageRating <= filter.MaxAverageRating)
                                                   && (filter.MinPrice == null || book.Price >= filter.MaxPrice)
                                                   && (filter.MaxPrice == null || book.Price <= filter.MaxPrice));

            switch (filter.OrderBy)
            {
                case BookOrder.Title:
                    filteredData = filteredData.OrderBy(book => book.Title);
                    break;
                case BookOrder.AuthorName:
                    filteredData = filteredData.OrderBy(book => book.AuthorFullName);
                    break;
                case BookOrder.BestAverageRating:
                    filteredData = filteredData.OrderByDescending(book => book.AverageRating);
                    break;
                case BookOrder.WorstAverageRating:
                    filteredData = filteredData.OrderBy(book => book.AverageRating);
                    break;
                case BookOrder.LowestPrice:
                    filteredData = filteredData.OrderBy(book => book.Price);
                    break;
                case BookOrder.HighestPrice:
                    filteredData = filteredData.OrderByDescending(book => book.Price);
                    break;
                default: throw new ArgumentException($"Filter's {nameof(filter.OrderBy)} property has an invalid value.");
            }

            return await PaginateAsync<BooksFilter, BookOrder, Book>(filter, filteredData);
        }

        /// <summary>
        /// Gets a paged list with Reviews found according to the search parameters.
        /// </summary>
        /// <remarks>
        /// If no page number is informed, it will return the first page. If no record limit (page size) is informed, it will be 100 records.
        /// </remarks>
        /// <response code="200"><pre>Returns a JSON of the page.</pre></response>
        /// <response code="404"><pre>If there is no Book record with the sent id.</pre></response>
        // GET: api/Books/5/Reviews
        [HttpGet("{id}/Reviews")]
        public async Task<ActionResult<PagedResponse<Review, ReviewOrder>>> ListReviews(long id, [FromQuery] ReviewsFilter filter)
        {
            if (!context.Book.Exists(id))
                return NotFound();

            var filteredData = context.Review.Where(review => review.Book.Id == id);

            switch (filter.OrderBy)
            {
                case ReviewOrder.MostRecent:
                    filteredData = filteredData.OrderBy(review => review.Creation);
                    break;
                case ReviewOrder.BestRating:
                    filteredData = filteredData.OrderByDescending(review => review.Rating);
                    break;
                case ReviewOrder.WorstRating:
                    filteredData = filteredData.OrderBy(review => review.Rating);
                    break;
                default: throw new ArgumentException($"Filter's {nameof(filter.OrderBy)} property has an invalid value.");
            }

            return await PaginateAsync<ReviewsFilter, ReviewOrder, Review>(filter, filteredData);
        }

        /// <summary>
        /// Creates a new Review for a Book. Body must contain JSON of the Review. Authorization is NOT needed.
        /// </summary>
        /// <response code="201"><pre>Succesfully created. A JSON of the new record is returned.</pre></response>
        /// <response code="400"><pre>If Creation field was filled, or any other validations didn't pass.</pre></response>
        /// <response code="404"><pre>If there is no Book record with the sent id.</pre></response>
        // POST: api/Books/5/Reviews
        [ProducesResponseType(201)]
        [HttpPost("{id}/Reviews")]
        public async Task<ActionResult<Review>> Post(long id, Review model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!context.Book.Exists(id))
                return NotFound();

            if (model.Creation != null)
                return BadRequest($"{nameof(model.Creation)} can't be filled. It is a read only property.");

            model.Creation = DateTime.Now;
            model.BookId = id;

            context.Review.Add(model);
            await context.SaveChangesAsync();

            model.CalculateAverageRating(context);

            model.IncludeChildren(context);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }
    }
}
