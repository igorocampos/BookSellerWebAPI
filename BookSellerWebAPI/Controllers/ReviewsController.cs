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
    public class ReviewsController : BaseController<Review>
    {
        public ReviewsController(BookSellerContext context) : base(context)
            => this.dbSet = context.Review;

        public override async Task<IActionResult> Put(long id, Review model)
        {
            var ret = await base.Put(id, model);

            if (ret is NoContentResult)
                model.CalculateAverageRating(context);

            return ret;
        }

        public override async Task<ActionResult<Review>> Delete(long id)
        {
            var ret = await base.Delete(id);

            if (ret.Value is Review)
                ret.Value.CalculateAverageRating(context);

            return ret;
        }

    }
}
