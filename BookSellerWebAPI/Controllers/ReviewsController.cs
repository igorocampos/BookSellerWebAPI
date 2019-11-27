using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookSellerWebAPI.Models;

namespace BookSellerWebAPI.Controllers
{
    public class ReviewsController : BaseController<Review>
    {
        public ReviewsController(BookSellerContext context) : base(context)
            => this.dbSet = context.Review;

        /// <summary>
        /// Edits the record that has the sent id. Body must contain JSON with new values, unchanged fields don't need to be sent. Authorization is needed.
        /// </summary>
        /// <response code="204"><pre>Succesfully edited. No content is returned.</pre></response>
        /// <response code="400"><pre>Trying to change a readonly field, or any of the validations didn't pass.</pre></response>
        /// <response code="401"><pre>Unauthorized to perform this operation.</pre></response>
        /// <response code="404"><pre>If there is no record with the sent id.</pre></response>
        public override async Task<IActionResult> Put(long id, Review model)
        {
            var ret = await base.Put(id, model);

            if (ret is NoContentResult)
                model.CalculateAverageRating(context);

            return ret;
        }

        /// <summary>
        /// Deletes the record that has the sent id. Authorization is needed.
        /// </summary>
        /// <response code="200"><pre>Returns a JSON of the deleted record.</pre></response>
        /// <response code="401"><pre>Unauthorized to perform this operation.</pre></response>
        /// <response code="404"><pre>If there is no record with the sent id.</pre></response>
        public override async Task<ActionResult<Review>> Delete(long id)
        {
            var ret = await base.Delete(id);

            if (ret.Value is Review)
                ret.Value.CalculateAverageRating(context);

            return ret;
        }

    }
}
