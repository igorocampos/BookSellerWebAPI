﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookSellerWebAPI.Models;

namespace BookSellerWebAPI.Models
{
    public class BookSellerContext : DbContext
    {
        public BookSellerContext (DbContextOptions<BookSellerContext> options)
            : base(options)
        {
        }

        public DbSet<BookSellerWebAPI.Models.Book> Book { get; set; }

        public DbSet<BookSellerWebAPI.Models.Review> Review { get; set; }
    }
}