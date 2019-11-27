using BookSellerWebAPI.Controllers.Filters;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookSellerWebAPI.Controllers
{
    public class AuthorsController : Controller<Author>
    {
        public AuthorsController(BookSellerContext context) : base(context)
            => dbSet = context.Author;

        /// <summary>
        /// Gets a paged list with Authors found according to the search parameters.
        /// </summary>
        /// <remarks>
        /// If no page number is informed, it will return the first page. If no record limit (page size) is informed, it will be 100 records.
        /// </remarks>
        /// <response code="200"><pre>Returns a JSON of the page.</pre></response>
        // GET: api/Authors
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Author, AuthorOrder>>> List([FromQuery] AuthorFilter filter)
        {
            var filteredData = context.Author.Where(author => (string.IsNullOrEmpty(filter.FirstName) || author.FirstName.ToUpper().Contains(filter.FirstName.ToUpper()))
                                                            && (string.IsNullOrEmpty(filter.LastName) || author.LastName.ToUpper().Contains(filter.LastName.ToUpper())));

            switch (filter.OrderBy)
            {
                case AuthorOrder.FirstName:
                    filteredData = filteredData.OrderBy(author => author.FirstName);
                    break;
                case AuthorOrder.LastName:
                    filteredData = filteredData.OrderBy(author => author.LastName);
                    break;
                default: throw new ArgumentException($"Filter's {nameof(filter.OrderBy)} property has an invalid value.");
            }

            return await PaginateAsync<AuthorFilter, AuthorOrder, Author>(filter, filteredData);
        }
    }
}
