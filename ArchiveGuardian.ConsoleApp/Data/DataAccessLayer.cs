using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;
using ArchiveGuardian.ConsoleApp.Data.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ArchiveGuardian.ConsoleApp.Behavior;


namespace ArchiveGuardian.ConsoleApp.Data;

internal static class DataAccessLayer
{
    internal async static Task<List<string>> GetFiles(int? pageNumber = null, int? pageSize = null)
    {
        try
        {
            using (var scope = Program.host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var query = db.Files.Select(x => x.Name).AsQueryable();

                if (pageNumber.HasValue && pageSize.HasValue)
                    query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);

                var data = await query.ToListAsync();

                return data;
            }
        }
        catch (Exception ex)
        {
            return new List<string>() { "An unexpected error occurred: " + ex.Message };
        }
    }

    internal static async Task<bool> AddFile(Files file)
    {
        try
        {
            using var scope = Program.host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //using var transaction = db.Database.BeginTransaction();

            int affectedRows = 0;


            file.TotalHash = CryptoHelper.CombineHashes(
                hash1: file.OwnHash,
                hash2: GetMostRecentHash()
                );

            db.Files.Add(file);
            affectedRows = db.SaveChanges();

            if (affectedRows > 0)
                return true;
            else
            {
                Console.WriteLine("Tried to add a file, but affected rows were 0.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred in AddFile: " + ex.Message);
            return false;
        }
    }

    internal static string GetMostRecentHash()
    {
        try
        {
            using (var scope = Program.host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var mostRecentHash = db.Files.OrderByDescending(x => x.ID).FirstOrDefault()?.TotalHash;
                return string.IsNullOrEmpty(mostRecentHash) ? string.Empty : mostRecentHash;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred in GetMostRecentHash: " + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Auto-migrates the db.
    /// </summary>
    internal static async void InitializeDatabase()
    {
        try
        {
            using (var scope = Program.host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred while migrating db: " + ex.Message);
        }
    }
}
