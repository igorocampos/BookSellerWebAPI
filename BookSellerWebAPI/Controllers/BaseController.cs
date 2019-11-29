using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSellerWebAPI.Models;
using BookSellerWebAPI.Controllers.Filters;
using System;
using BookSellerWebAPI.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace BookSellerWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BaseController<T> : ControllerBase where T : BaseModel
    {
        protected readonly BookSellerContext context;
        protected DbSet<T> dbSet;

        public BaseController(BookSellerContext context)
            => this.context = context;

        /// <summary>
        /// Gets the record that has the sent id.
        /// </summary>
        /// <response code="200"><pre>Returns a JSON of the record.</pre></response>
        /// <response code="404"><pre>If there is no record with the sent id.</pre></response>
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

        /// <summary>
        /// Deletes the record that has the sent id. Authorization is needed.
        /// </summary>
        /// <response code="200"><pre>Returns a JSON of the deleted record.</pre></response>
        /// <response code="401"><pre>Unauthorized to perform this operation.</pre></response>
        /// <response code="404"><pre>If there is no record with the sent id.</pre></response>
        // DELETE: api/controller/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public virtual async Task<ActionResult<T>> Delete(long id)
        {
            var model = await dbSet.FindAsync(id);
            if (model is null)
                return NotFound($"There is no record with id {id}");

            dbSet.Remove(model);
            await context.SaveChangesAsync();

            return Ok(model);
        }

        /// <summary>
        /// Edits the record that has the sent id. Body must contain JSON with new values, unchanged fields don't need to be sent. Authorization is needed.
        /// </summary>
        /// <response code="204"><pre>Succesfully edited. No content is returned.</pre></response>
        /// <response code="400"><pre>Trying to change a readonly field, or any of the validations didn't pass.</pre></response>
        /// <response code="401"><pre>Unauthorized to perform this operation.</pre></response>
        /// <response code="404"><pre>If there is no record with the sent id.</pre></response>
        // PUT: api/controller/5
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(204)]
        public virtual async Task<IActionResult> Put(long id, T model)
        {
            if (model.Id == default)
                model.Id = id;

            if (id != model.Id)
                return BadRequest("Sent model has different id from url.");

            T old = dbSet.Find(id);

            if (old is null)
                return NotFound();

            //Overwrite all properties holding default values with current data, because user is not required to send unchanged data.
            foreach (var prop in typeof(T).GetProperties())
            {
                var oldValue = prop.GetValue(old);
                var newValue = prop.GetValue(model);

                if (prop.CanWrite && newValue.IsDefault(prop.PropertyType))
                    prop.SetValue(model, newValue = oldValue);

                //if property has Editable = false attribute, returns BadRequest when user is trying to change its value
                if (prop.GetCustomAttributes(false)?.OfType<EditableAttribute>().FirstOrDefault()?.AllowEdit == false && oldValue != null && !oldValue.Equals(newValue))
                    return BadRequest($"It is not allowed to change the value of {prop.Name} property.");
            }

            if (!model.Validate(out var error, context))
                return BadRequest(error);

            context.Entry(old).State = EntityState.Detached;

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

        /// <summary>
        /// Creates a new record. Body must contain JSON of the model. Authorization is needed.
        /// </summary>
        /// <response code="201"><pre>Succesfully created. A JSON of the new record is returned.</pre></response>
        /// <response code="400"><pre>Any of the validations didn't pass.</pre></response>
        /// <response code="401"><pre>Unauthorized to perform this operation.</pre></response>
        // POST: api/controller
        [HttpPost]
        [ProducesResponseType(201)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<T>> Post(T model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!model.Validate(out var error, context))
                return BadRequest(error);

            dbSet.Add(model);
            await context.SaveChangesAsync();

            model.IncludeChildren(context);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }
    }
}
