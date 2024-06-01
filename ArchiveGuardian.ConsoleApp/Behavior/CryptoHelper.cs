using ArchiveGuardian.ConsoleApp.Data;
using ArchiveGuardian.ConsoleApp.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveGuardian.ConsoleApp.Behavior;

internal static class CryptoHelper
{
    /// <summary>
    /// Calculates the SHA256 hash of a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>SHA256 hash as a hex string</returns>
    internal static string CalculateFileHash(string filePath)
    {
        var sha256 = SHA256.Create();
        var stream = File.OpenRead(filePath);

        var hash = sha256.ComputeHash(stream);

        stream.Dispose();
        sha256.Dispose();

        return BitConverter.ToString(hash).Replace("-", "");
    }

    /// <summary>
    /// Appends two strings and calculates their SHA256 hash
    /// </summary>
    /// <param name="hash1">The new file's own hash</param>
    /// <param name="hash2">The most recent total hash</param>
    /// <returns>The SHA256 hash of the concatenated string as a hex string.</returns>
    internal static string CombineHashes(string hash1, string hash2)
    {
        if (hash2 == string.Empty)
            return hash1;

        var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hash1 + hash2));
        sha256.Dispose();
        return BitConverter.ToString(hashBytes).Replace("-", "");
    }

    internal static string GetPreviousTotalHash(Files file)
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var previousFile = db.Files.SingleOrDefault(f => f.ID == file.ID - 1);

            if (previousFile == null)
                return file.OwnHash;
            else
                return previousFile.TotalHash;
        }
    }
}
