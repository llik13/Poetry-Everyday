using DataAccess.Context;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PoetryDbContextFactory : IDesignTimeDbContextFactory<PoetryDbContext>
    {
        public PoetryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PoetryDbContext>();
            var connectionString = "Server=localhost;Database=poetry;User=root;Password=0000;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new PoetryDbContext(optionsBuilder.Options);
        }
    }
}
