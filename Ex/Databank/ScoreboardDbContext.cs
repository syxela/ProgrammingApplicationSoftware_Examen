using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Ex.Databank
{
    public class ScoreboardDbContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source = ScoreboardFile.db");
        }

        public DbSet<User> Users { get; set; }

        //method om id van user te krijgen
        public User GetUserId(int userId)
        {
            return Users.FirstOrDefault(u => u.Id == userId);
        }
    }
}
