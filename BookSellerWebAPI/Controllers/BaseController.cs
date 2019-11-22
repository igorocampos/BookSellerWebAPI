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
    [ApiController]
    [Route("api")]
    public class BaseController<T> : ControllerBase where T : BaseModel
    {
        protected readonly BookSellerContext context;
        protected DbSet<T> dbSet;

        public BaseController(BookSellerContext context)
            => this.context = context;

        protected async Task<PagedResponse<T, V>> FilterAsync<U, V>(U filter, IQueryable<T> filteredData) where U : BaseFilter<V> where V : Enum
        {
            if (filter.Limit <= 0)
                filter.Limit = 1;

            List<T> filterData = await filteredData
                              .Skip((filter.Page - 1) * filter.Limit)
                              .Take(filter.Limit).ToListAsync();

            //Get the data for the current page
            var result = new PagedResponse<T, V>();
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
        [HttpGet("[controller]/{id}")]
        public async Task<ActionResult<T>> Get(long id)
        {
            var model = await dbSet.FindAsync(id);

            if (model is null)
                return NotFound();

            return model;
        }

        // POST: api/controller
        [HttpPost("[controller]")]
        public async Task<ActionResult<T>> Post(T model)
        {
            dbSet.Add(model);
            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }

        // DELETE: api/controller/5
        [HttpDelete("[controller]/{id}")]
        public async Task<ActionResult<T>> Delete(long id)
        {
            var model = await dbSet.FindAsync(id);
            if (model is null)
                return NotFound();

            dbSet.Remove(model);
            await context.SaveChangesAsync();

            return model;
        }

        // PUT: api/controller/5
        [HttpPut("[controller]/{id}")]
        public async Task<IActionResult> Put(long id, T model)
        {
            if (id != model.Id)
                return BadRequest();

            context.Entry(model).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists<T>(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        protected bool Exists<W>(long id, IQueryable set = null) where W : BaseModel
            => (set ?? this.dbSet).Cast<W>().Any(e => e.Id == id);
    }
}
