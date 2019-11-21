using BookSellerWebAPI.Models;
using System;
using System.Collections.Generic;

namespace BookSellerWebAPI.Controllers.Filters
{
    public class BaseFilter
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 100;
    }

    public class PagedResponse<T, U> where T : BaseModel where U : Enum
    {
        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public U OrderedBy { get; set; }
    }
}
