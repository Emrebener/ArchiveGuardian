using ArchiveGuardian.ConsoleApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ArchiveGuardian.ConsoleApp.Data;

internal class ApplicationDbContext : DbContext
{
    public DbSet<Files> Files { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {

    }
}
