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
    public class BooksController : BaseController<Book>
    {
        public BooksController(BookSellerContext context) : base(context)
            => this.dbSet = context.Book;

        // GET: api/Books
        [HttpGet("Books")]
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

            return await PaginateAsync<BooksFilter, BookOrder>(filter, filteredData);
        }
    }
}
