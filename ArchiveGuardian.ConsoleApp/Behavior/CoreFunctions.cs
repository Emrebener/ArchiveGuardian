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

internal static class CoreFunctions
{
    internal static async void ArsiveEkle(string? yol)
    {
        if (string.IsNullOrEmpty(yol))
        {
            Console.WriteLine("Arsive ekleme komutu icin \"yol\" parametresi zorunludur.");
            return;
        }

        if (!File.Exists(yol))
        {
            Console.WriteLine($"'{yol}' adresinde bir dosya bulunamadi.");
            return;
        }

        try
        {
            string fileName = Path.GetFileName(yol);
            string fileHash = CryptoHelper.CalculateFileHash(yol);

            var file = new Files
            {
                Name = fileName,
                Path = yol,
                OwnHash = fileHash
            };

            bool isSuccess = await DataAccessLayer.AddFile(file);

            if (isSuccess)
            {
                Console.WriteLine($"'{fileName}' isimli dosya arsive basarili bir sekilde eklendi.");
            }
            else
            {
                Console.WriteLine($"'{fileName}' isimli dosya arsive eklenemedi.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ArsiveEkle - beklenmedik bir hata olustu: " + ex.Message);
        }
    }

    /// <summary>
    /// Outputs a list of all archive items.
    /// </summary>
    /// <param name="pageSize">Number of lines in each page</param>
    /// <param name="pageNumber">Number of page to view</param>
    internal static void IcerikListele(int? satirSayisi, int? sayfaNo)
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = db.Files.OrderBy(f => f.ID);

            if (satirSayisi.HasValue && sayfaNo.HasValue)
            {
                // Paginated mode
                int pageSize = satirSayisi.Value;
                int pageNumber = sayfaNo.Value;
                var paginatedFiles = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                SystemFunctions.PrintBlue($"Sayfa: {pageNumber}, Satir sayisi: {pageSize}.");
                foreach (var file in paginatedFiles)
                {
                    Console.WriteLine($"ID: {file.ID}, DosyaAdi: {file.Name}, Yol: {file.Path}, OwnHash: {file.OwnHash}, TotalHash: {file.TotalHash}");
                }
            }
            else
            {
                // No pagination
                var allFiles = query.ToList();
                foreach (var file in allFiles)
                {
                    Console.WriteLine($"ID: {file.ID}, DosyaAdi: {file.Name}, Yol: {file.Path}, OwnHash: {file.OwnHash}, TotalHash: {file.TotalHash}");
                }
                SystemFunctions.PrintBlue($"Arsivdeki toplam dosya sayisi: {allFiles.Count}.");
            }
        }
    }

    /// <summary>
    /// Recalculates the "CalcHash" value for each existing line, starting from the first line.
    /// </summary>
    internal static void ZinciriTekrarHesapla()
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var files = db.Files.OrderBy(f => f.ID).ToList();
            string previousTotalHash = string.Empty;

            foreach (var file in files)
            {
                file.OwnHash = CryptoHelper.CalculateFileHash(file.Path);

                file.TotalHash = string.IsNullOrEmpty(previousTotalHash)
                    ? file.OwnHash
                    : CryptoHelper.CombineHashes(file.OwnHash, previousTotalHash);

                previousTotalHash = file.TotalHash;
            }

            var affectedRows = db.SaveChanges();
            Console.WriteLine(affectedRows + " satir guncellenmistir.");
        }
    }


}
