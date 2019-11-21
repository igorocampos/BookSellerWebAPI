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
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookSellerContext context;

        public BooksController(BookSellerContext context)
            => this.context = context;

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Book, BookOrder>>> List([FromQuery] BooksFilter filter)
        {
            if (filter.Limit <= 0)
                filter.Limit = 1;

            var totalFiltered = context.Book.Where(book => (string.IsNullOrEmpty(filter.Title) || book.Title.ToUpper().Contains(filter.Title.ToUpper()))
                                                   && (string.IsNullOrEmpty(filter.AuthorName) || book.AuthorName.ToUpper().Contains(filter.AuthorName.ToUpper()))
                                                   && (filter.MinAverageRating == null || book.AverageRating >= filter.MinAverageRating)
                                                   && (filter.MaxAverageRating == null || book.AverageRating <= filter.MaxAverageRating)
                                                   && (filter.MinPrice == null || book.Price >= filter.MaxPrice)
                                                   && (filter.MaxPrice == null || book.Price <= filter.MaxPrice));

            switch (filter.OrderBy)
            {
                case BookOrder.Title:
                    totalFiltered = totalFiltered.OrderBy(book => book.Title);
                    break;
                case BookOrder.AuthorName:
                    totalFiltered = totalFiltered.OrderBy(book => book.AuthorName);
                    break;
                case BookOrder.BestAverageRating:
                    totalFiltered = totalFiltered.OrderByDescending(book => book.AverageRating);
                    break;
                case BookOrder.WorstAverageRating:
                    totalFiltered = totalFiltered.OrderBy(book => book.AverageRating);
                    break;
                case BookOrder.LowestPrice:
                    totalFiltered = totalFiltered.OrderBy(book => book.Price);
                    break;
                case BookOrder.HighestPrice:
                    totalFiltered = totalFiltered.OrderByDescending(book => book.Price);
                    break;
                default: throw new ArgumentException($"Filter's {nameof(filter.OrderBy)} property has an invalid value.");
            }

            List<Book> filterData = await totalFiltered
                              .Skip((filter.Page - 1) * filter.Limit)
                              .Take(filter.Limit).ToListAsync();

            //Get the data for the current page
            var result = new PagedResponse<Book, BookOrder>();
            result.Items = filterData;
            result.CurrentPage = filter.Page;
            var countTotalFiltered = await totalFiltered.CountAsync();
            int totalPages = countTotalFiltered == 0 ? 1 : countTotalFiltered / filter.Limit;

            //if it is not a round division, must have an aditional page
            if (countTotalFiltered % filter.Limit != 0)
                totalPages++;

            result.TotalPages = totalPages;
            result.OrderedBy = filter.OrderBy;

            return result;
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(long id)
        {
            var book = await context.Book.FindAsync(id);

            if (book is null)
                return NotFound();

            return book;
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(long id, Book book)
        {
            if (id != book.Id)
                return BadRequest();

            context.Entry(book).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            context.Book.Add(book);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Book>> DeleteBook(long id)
        {
            var book = await context.Book.FindAsync(id);
            if (book is null)
                return NotFound();

            context.Book.Remove(book);
            await context.SaveChangesAsync();

            return book;
        }

        private bool BookExists(long id)
            => context.Book.Any(e => e.Id == id);

    }
}
