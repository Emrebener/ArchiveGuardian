using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArchiveGuardian.ConsoleApp.Behavior;

internal static class SystemFunctions
{
    /// <summary>
    /// Prompts the user to provide a command and any necessary additional arguments for the command, and parses it into a dictionary.
    /// </summary>
    /// <param name="args">Represents the arguments that were passed as command line arguments to the program, which may or may not be empty.</param>
    /// <returns>Prepared and parsed arguments dictionary</returns>
    internal static Dictionary<string, string> ReadAndParseArgs(string[]? args)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (args != null && args.Length > 0)
            parameters = ParseArguments(args);

        while (parameters.Count == 0) // keep asking until you get arguments
        {
            //Console.WriteLine("Bir komut belirtiniz, veya \"Yardim\" yaziniz. Cikmak icin \"Cikis\" yaziniz.");
            Console.WriteLine("Bir komut belirtiniz, veya \"Yardim\" yaziniz. Cikmak icin \"Cikis\" yaziniz.");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            args = ParseCommandLine(input);
            if (args[0].ToLower() == "yardim")
            {
                Yardim();
                continue;
            }

            parameters = ParseArguments(args);
        }

        return parameters;
    }

    /// <summary>
    /// Parses a command line string into an array of arguments, respecting quoted strings.
    /// </summary>
    /// <param name="commandLine">The command line string.</param>
    /// <returns>An array of arguments.</returns>
    private static string[] ParseCommandLine(string commandLine)
    {
        var argsList = new List<string>();
        var regex = new Regex(@"(""(?:[^""]|"""")*""|[^ ]+)", RegexOptions.Compiled);

        foreach (Match match in regex.Matches(commandLine))
        {
            argsList.Add(match.Value);
        }

        return argsList.ToArray();
    }

    /// <summary>
    /// Parses the array of arguments into a dictionary.
    /// </summary>
    /// <param name="args">The array of arguments.</param>
    /// <returns>A dictionary of parsed arguments.</returns>
    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (args.Length > 0)
        {
            parameters["method"] = args[0].Trim('"');
        }

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                var key = args[i].TrimStart('-');
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    parameters[key] = args[++i].Trim('"');
                }
                else
                {
                    parameters[key] = string.Empty;
                }
            }
        }

        return parameters;
    }

    internal static void ExecuteRelevantMethod(Dictionary<string, string> args)
    {
        if (!args.ContainsKey("method"))
            Console.WriteLine("Komut bulunamadi...");

        string methodName = args["method"].ToLower();

        switch (methodName)
        {
            case "arsiveekle":
                CoreFunctions.ArsiveEkle(
                    yol: args.ContainsKey("yol") ? args["yol"] : null
                    );
                break;
            case "iceriklistele":
                CoreFunctions.IcerikListele(
                    satirSayisi: args.ContainsKey("satirSayisi") ? Convert.ToInt32(args["satirSayisi"]) : null,
                    sayfaNo: args.ContainsKey("sayfaNo") ? Convert.ToInt32(args["sayfaNo"]) : null
                    );
                break;
            case "zinciritekrarhesapla":
                CoreFunctions.ZinciriTekrarHesapla();
                break;
            case "dosyayidogrula":
                DataIntegrityFunctions.DosyayiDogrula(
                    dosyaAdi: args.ContainsKey("dosyaAdi") ? args["dosyaAdi"] : null,
                    id: args.ContainsKey("id") ? Convert.ToInt32(args["id"]) : null
                    );
                break;
            case "arsividogrula":
                DataIntegrityFunctions.ArsiviDogrula();
                break;
            case "testverisiolustur":
                TestFunctions.TestVerisiOlustur(
                    dosyaSayisi: args.ContainsKey("dosyaSayisi") ? Convert.ToInt32(args["dosyaSayisi"]) : null,
                    yol: args.ContainsKey("yol") ? args["yol"] : null
                    );
                break;
            case "dosyayamudahaleet":
                TestFunctions.DosyayaMudahaleEt(
                    dosyaAdi: args.ContainsKey("dosyaAdi") ? args["dosyaAdi"] : null
                    );
                break;
            default:
                Console.WriteLine("Komut bulunamadi...");
                break;
        }
    }

    /// <summary>
    /// Prints a list of all functions as well as their parameters.
    /// </summary>
    internal static void Yardim()
    {
        Console.WriteLine("\n");
        PrintBlue("SISTEM KOMUTLARI:");
        Console.WriteLine("`Yardim` -> Komutlari listeler.");
        Console.WriteLine("`Cikis` -> Programi kapatir.");

        Console.WriteLine("\n");
        PrintBlue("ANA KOMUTLAR:");
        Console.WriteLine("`ArsiveEkle -yol` -> Arsive bir dosya veya klasorun butun icerigini ekler.");
        Console.WriteLine("`IcerikListele -satirSayisi? -sayfaNo?` -> Arsivdeki butun dosyalarin listesini verir. \"satirSayisi\" ve \"sayfaNo\" parametreleri opsiyoneldir.");
        Console.WriteLine("`ZinciriTekrarHesapla` -> Arsivdeki hash zincirini bastan sona tekrar hesaplar.");

        Console.WriteLine("\n");
        PrintBlue("VERI BUTUNLUGU KOMUTLARI:");
        Console.WriteLine("`DosyayiDogrula -dosyaAdi? -id? -> Bir dosyanin veri butunlugunu kontrol eder. \"-dosyaAdi\" veya \"-id\" parametrelerinden en az birisi gereklidir. Ikisi de verilirse dosya adi baz alinir.");
        Console.WriteLine("`ArsiviDogrula` -> Arsiv genelinde veri butunlugu kontrolu yapar. Veri butunlugu sorunu tespit ederse sorunun hangi satirda oldugunu belirtir.");

        Console.WriteLine("\n");
        PrintBlue("TEST KOMUTLARI:");
        Console.WriteLine("`TestVerisiOlustur -dosyaSayisi -yol` -> Test amaclari icin arsive bir miktar ornek ici dolu metin belgesi kaydeder.");
        Console.WriteLine("`DosyayaMudahaleEt -dosyaAdi` -> Belirtilen dosyanin iceriginde degisiklik yaparak veri butunlugunu bozar.\n");
    }


    #region COLORFUL PRINTS
    internal static void PrintGreen(string msg)
    {
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(msg);
        Console.BackgroundColor = ConsoleColor.Black;
    }
    internal static void PrintRed(string msg)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.BackgroundColor = ConsoleColor.Black;
    }
    internal static void PrintYellow(string msg)
    {
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(msg);
        Console.BackgroundColor = ConsoleColor.Black;
    }
    internal static void PrintBlue(string msg)
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(msg);
        Console.BackgroundColor = ConsoleColor.Black;
    }
    #endregion

    /// <summary>
    /// Prints the provided args dictionary for debugging purposes.
    /// </summary>
    /// <param name="args">Arguments dictionary</param>
    internal static string PrintArgs(Dictionary<string, string> args)
    {
        //foreach (var arg in args)
        //{
        //    Console.WriteLine($"{arg.Key}: {arg.Value}");
        //}
        var list = args.Select(x => x.Value).ToList();
        return string.Join(" ", list);
    }
}
