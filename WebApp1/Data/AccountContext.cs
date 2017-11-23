using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApp1.Models;

namespace WebApp1.Data
{
    public class AccountContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<AccountContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}