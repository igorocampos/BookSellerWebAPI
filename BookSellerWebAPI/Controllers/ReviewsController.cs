using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSellerWebAPI.Models;
using BookSellerWebAPI.Controllers.Filters;
using System;

namespace BookSellerWebAPI.Controllers
{
    public class ReviewsController : BaseController<Review>
    {
        public ReviewsController(BookSellerContext context) : base(context)
            => this.dbSet = context.Review;

        // GET: api/Books/5/Reviews
        [HttpGet("Books/{id}/Reviews")]
        public async Task<ActionResult<PagedResponse<Review, ReviewOrder>>> ListReviews(long id, [FromQuery] ReviewsFilter filter)
        {
            if (!Exists<Book>(id, context.Book))
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

            return await FilterAsync<ReviewsFilter, ReviewOrder>(filter, filteredData);
        }
    }
}
