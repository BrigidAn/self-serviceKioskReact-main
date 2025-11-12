using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Data
{
    public class ApplicationDBContext : DBContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        {
        }      
        public DbSet <User> Users { get; set; }
    }
}