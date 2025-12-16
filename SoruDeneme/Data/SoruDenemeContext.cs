using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoruDeneme.Models;

namespace SoruDeneme.Data
{
    public class SoruDenemeContext : DbContext
    {
        public SoruDenemeContext (DbContextOptions<SoruDenemeContext> options)
            : base(options)
        {
        }

        public DbSet<SoruDeneme.Models.Question> Question { get; set; } = default!;
        public DbSet<SoruDeneme.Models.Quiz> Quiz { get; set; } = default!;
    }
}
