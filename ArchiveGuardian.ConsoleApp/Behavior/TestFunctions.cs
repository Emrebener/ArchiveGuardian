using ArchiveGuardian.ConsoleApp.Data.Entities;
using ArchiveGuardian.ConsoleApp.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveGuardian.ConsoleApp.Behavior;

internal static class TestFunctions
{
    /// <summary>
    /// Generates a number of text files that contain random text by the specified amount.
    /// </summary>
    /// <param name="numOfFiles">Number of files to generate</param>
    /// <param name="path">File or folder path</param>
    internal static async void TestVerisiOlustur(int? dosyaSayisi, string? yol)
    {
        if (!dosyaSayisi.HasValue || string.IsNullOrEmpty(yol))
        {
            Console.WriteLine("RastgeleDosyalarOlusturVeArsiveEkle komutu icin dosya sayisi ve yol belirtilmesi gerekmektedir.");
            return;
        }

        _ = Directory.CreateDirectory(yol);

        for (int i = 0; i < dosyaSayisi.Value; i++)
        {
            string fileName = $"OrnekDosya_{Guid.NewGuid().ToString().Substring(28)}.txt";
            string filePath = Path.Combine(yol, fileName);

            string fileContent = OrnekYaziOlustur(100); // Generate 100-word dummy text
            using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                sw.Write(fileContent);
                sw.Flush();
            }
            //File.WriteAllText(filePath, fileContent);

            string fileHash = CryptoHelper.CalculateFileHash(filePath);

            var file = new Files
            {
                Name = fileName,
                Path = filePath,
                OwnHash = fileHash
            };

            if (await DataAccessLayer.AddFile(file))
                Console.WriteLine($"Dosya {i+1} olusturuldu ve arsive eklendi.");

            // Thread.Sleep(TimeSpan.FromSeconds(0.1));
        }
    }

    /// <summary>
    /// Purposefully tampers with the specified file in the archive, ruining its data integrity.
    /// The purpose is to later prove that this tampering can be detected by the system.
    /// </summary>
    /// <param name="fileName">Name of the file to be tampered with (optional)</param>
    internal static void DosyayaMudahaleEt(string? dosyaAdi)
    {
        using (var scope = Program.host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Files? fileToTamper;

            if (string.IsNullOrEmpty(dosyaAdi))
            {
                var allFiles = db.Files.ToList();
                if (allFiles.Count == 0)
                {
                    Console.WriteLine("Arsivde mudahale edilecek dosya bulunamadi.");
                    return;
                }

                // Pick a random file from the archive
                Random rnd = new Random();
                fileToTamper = allFiles[rnd.Next(allFiles.Count)];
            }
            else
            {
                // Retrieve the specified file by its name
                fileToTamper = db.Files.SingleOrDefault(f => f.Name.StartsWith(dosyaAdi));

                if (fileToTamper == null)
                {
                    Console.WriteLine($"'{dosyaAdi}' adli dosya arsivde bulunamadi.");
                    return;
                }
            }

            // Tamper with the file
            TamperFileContents(fileToTamper.Path);
            //Console.WriteLine($"'{fileToTamper.Name}' adli dosyanin icerigine mudahale edildi.");
            SystemFunctions.PrintBlue($"'{fileToTamper.Name}' adli dosyanin icerigine mudahale edildi.");
        }
    }

    /// <summary>
    /// Tamper with the contents of the file at the specified path.
    /// </summary>
    /// <param name="filePath">Path of the file to be tampered with</param>
    private static void TamperFileContents(string filePath)
    {
        const string tamperText = "---ICERIGE MUDAHALE EDILDI---";
        using (var writer = new StreamWriter(filePath, append: true))
        {
            writer.WriteLine(tamperText);
        }
    }

    /// <summary>
    /// Generates dummy text to be used in the dummy text files. Not meant for direct invocation. Gets used by "GenerateAndArchiveDummyText" method.
    /// </summary>
    /// <param name="numOfWords">Number of words to be generated</param>
    /// <returns>Dummy text in desired length</returns>
    private static string OrnekYaziOlustur(int kelimeSayisi)
    {
        // Lorem ipsum word pool
        string[] loremIpsumWords = new string[]
        {
        "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit",
        "sed", "do", "eiusmod", "tempor", "incididunt", "ut", "labore", "et", "dolore",
        "magna", "aliqua", "ut", "enim", "ad", "minim", "veniam", "quis", "nostrud",
        "exercitation", "ullamco", "laboris", "nisi", "ut", "aliquip", "ex", "ea",
        "commodo", "consequat", "duis", "aute", "irure", "dolor", "in", "reprehenderit",
        "in", "voluptate", "velit", "esse", "cillum", "dolore", "eu", "fugiat",
        "nulla", "pariatur", "excepteur", "sint", "occaecat", "cupidatat", "non",
        "proident", "sunt", "in", "culpa", "qui", "officia", "deserunt", "mollit",
        "anim", "id", "est", "laborum"
        };

        StringBuilder result = new StringBuilder(); // StringBuilder is used for efficient string concatenation

        Random rnd = new Random();

        for (int i = 0; i < kelimeSayisi; i++)
        {
            // Pick a random word from the lorem ipsum words list
            string word = loremIpsumWords[rnd.Next(loremIpsumWords.Length)];
            result.Append(word);

            // Add a space after each word except the last one
            if (i < kelimeSayisi - 1)
            {
                result.Append(" ");
            }
        }

        return result.ToString();
    }
}
