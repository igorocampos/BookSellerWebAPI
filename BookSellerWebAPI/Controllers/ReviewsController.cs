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
    }
}
