﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore;

namespace AWSBlogCSharp.Database
{
    public class BlogPostContext : DbContext
    {
        public BlogPostContext() { }

        public BlogPostContext(DbContextOptions<BlogPostContext> options) : base(options)
        { }

        /**
         * A table of blog posts, keyed on Id and Version.
         */
        public DbSet<DBBlogPost> BlogPost { get; set; }

        /**
         * A table used to auto generate blog ids.  Used for creation only.
         */
        public DbSet<DBBlogId> BlogIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DBBlogPost>()
                .HasKey(c => new { c.Id, c.Version });
        }

    }
}
