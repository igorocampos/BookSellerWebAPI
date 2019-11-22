using System.Collections.Generic;
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
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<T> : ControllerBase where T : BaseModel
    {
        protected readonly BookSellerContext context;
        protected DbSet<T> dbSet;

        public BaseController(BookSellerContext context)
            => this.context = context;

        // DELETE: api/controller/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<T>> Delete(long id)
        {
            var model = await dbSet.FindAsync(id);
            if (model is null)
                return NotFound($"There is no record with id {id}");

            dbSet.Remove(model);
            await context.SaveChangesAsync();

            return model;
        }

        // PUT: api/controller/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, T model)
        {
            if (!model.Validate(out var error, context))
                return BadRequest(error);

            if (id != model.Id)
                return BadRequest("Sent model has different id from url.");

            context.Entry(model).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!dbSet.Exists(id))
                    return NotFound($"There is no record with id {id}");
                else
                    throw;
            }

            return NoContent();
        }
    }

    public class Controller<T> : BaseController<T> where T : BaseModel
    {
        public Controller(BookSellerContext context) : base(context) { }

        protected async Task<PagedResponse<W, V>> PaginateAsync<U, V, W>(U filter, IQueryable<W> filteredData) where U : BaseFilter<V> where V : Enum where W : BaseModel
        {
            if (filter.Limit <= 0)
                filter.Limit = 1;

            List<W> filterData = await filteredData
                              .Skip((filter.Page - 1) * filter.Limit)
                              .Take(filter.Limit).ToListAsync();

            //Get the data for the current page
            var result = new PagedResponse<W, V>();
            result.Items = filterData;
            result.CurrentPage = filter.Page;
            var countTotalFiltered = await filteredData.CountAsync();
            int totalPages = countTotalFiltered == 0 ? 1 : countTotalFiltered / filter.Limit;

            //if it is not a round division, must have an aditional page
            if (countTotalFiltered % filter.Limit != 0)
                totalPages++;

            result.TotalPages = totalPages;
            result.OrderedBy = filter.OrderBy;

            return result;
        }

        // GET: api/controller/5
        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(long id)
        {
            var model = await dbSet.FindAsync(id);

            if (model is null)
                return NotFound($"There is no record with id {id}");

            model.IncludeChildren(context);

            return model;
        }

        // POST: api/controller
        [HttpPost]
        public async Task<ActionResult<T>> Post(T model)
        {
            if (!model.Validate(out var error, context))
                return BadRequest(error);

            dbSet.Add(model);
            await context.SaveChangesAsync();

            model.IncludeChildren(context);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }
    }
}
