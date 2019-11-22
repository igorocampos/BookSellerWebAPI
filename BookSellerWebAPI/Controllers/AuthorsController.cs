using BookSellerWebAPI.Controllers.Filters;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookSellerWebAPI.Controllers
{
    public class AuthorsController : BaseController<Author>
    {
        public AuthorsController(BookSellerContext context) : base(context)
            => dbSet = context.Author;

        // GET: api/Authors
        [HttpGet("Authors")]
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

            return await PaginateAsync<AuthorFilter, AuthorOrder>(filter, filteredData);
        }
    }
}
