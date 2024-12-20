using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
        : base(dbContextOptions)
        {

        }
        public DbSet<Employee> Employees{ get; set; }
        public DbSet<ItemCategory> ItemCategories{ get; set; }
        public DbSet<ItemClassification> ItemClassifications{ get; set; }
        public DbSet<ItemInstance> ItemInstances{ get; set; }
        public DbSet<RequisitionedItem> RequisitionedItems{ get; set; }

    }
}