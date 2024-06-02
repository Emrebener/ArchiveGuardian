using ArchiveGuardian.ConsoleApp.Data;
using ArchiveGuardian.ConsoleApp.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveGuardian.ConsoleApp.Behavior;

internal static class DataIntegrityFunctions
{
    /// <summary>
    /// Looks up a file in the archive, and checks its hash validity.
    /// Doesn't perform a global archive check, only checks the file itself.
    /// Parameterize the input.S
    /// </summary>
    /// <param name="fileName">Name of the file</param>

    internal static void DosyayiDogrula(string? dosyaAdi, int? id)
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            #region DOSYAYI BUL

            Files? file;

            if (!string.IsNullOrEmpty(dosyaAdi))
                file = db.Files.SingleOrDefault(f => f.Name == dosyaAdi);
            else if (id.HasValue)
                file = db.Files.SingleOrDefault(f => f.ID == id.Value);
            else
            {
                SystemFunctions.PrintYellow("Dosya adi veya ID belirtilmelidir.");
                return;
            }
            if (file == null)
            {
                SystemFunctions.PrintYellow("Belirtilen dosya arsivde bulunamadi.");
                return;
            }

            #endregion

            #region VERI BUTUNLUGUNU DOGRULA

            // OwnHash tekrar hesapla
            string recalculatedOwnHash = CryptoHelper.CalculateFileHash(file.Path);

            // TotalHash tekrar hesapla
            string lookedUpTotalHash = CryptoHelper.GetPreviousTotalHash(file);
            string recalculatedTotalHash = CryptoHelper.CombineHashes(recalculatedOwnHash, lookedUpTotalHash);

            // OwnHash ve TotalHash degerlerini kontrol et
            if (file.OwnHash != recalculatedOwnHash || file.TotalHash != recalculatedTotalHash)
                SystemFunctions.PrintRed($"{file.Name} adli dosyanin veri butunlugunun bozuk oldugu tespit edilmistir.");
            else
                SystemFunctions.PrintGreen($"'{file.Name}' dosyasinin veri butunlugunu dogrulanmistir.");

            #endregion
        }
    }

    /// <summary>
    /// Validates the overall data integrity of the archive by checking each item one by one.
    /// </summary>
    internal static void ArsiviDogrula()
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var files = db.Files.OrderBy(f => f.ID).ToList();

            if (files.Count == 0)
            {
                SystemFunctions.PrintYellow("Arsivde kayitli dosya bulunmamaktadir.");
                return;
            }

            Console.WriteLine($"{files.Count} dosyanin veri butunlugu kontrol ediliyor...");

            string previousTotalHash = string.Empty;
            foreach (var file in files)
            {
                string recalculatedOwnHash = CryptoHelper.CalculateFileHash(file.Path); // recalculated OwnHash
                string recalculatedTotalHash = string.IsNullOrEmpty(previousTotalHash)
                    ? recalculatedOwnHash
                    : CryptoHelper.CombineHashes(recalculatedOwnHash, previousTotalHash); // recalculated TotalHash

                if (recalculatedOwnHash != file.OwnHash || recalculatedTotalHash != file.TotalHash)
                {
                    SystemFunctions.PrintRed($"Veri butunlugu bozuk dosya tespit edildi; {file.Name}");
                    return;
                }

                previousTotalHash = recalculatedTotalHash; // update "previous total hash" to be used in next iteration
            }

            SystemFunctions.PrintGreen("Arsivdeki butun dosyalarin veri butunlugunu korundugu dogrulanmistir.");
        }

    }
}
