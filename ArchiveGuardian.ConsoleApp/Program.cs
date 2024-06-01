using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Xml;
using ArchiveGuardian.ConsoleApp.Data;
using ArchiveGuardian.ConsoleApp.Behavior;
using Microsoft.Extensions.Logging;

namespace ArchiveGuardian.ConsoleApp;

internal class Program
{
    /*
        TYZM-691
        Emre Bener
    */


    internal static IHost host;
    static void Main(string[] args)
    {
        host = CreateHostBuilder(args).Build();
        bool firstExecution = true;

        while (true)
        {
            var parameters = SystemFunctions.ReadAndParseArgs(args: firstExecution ? args : null); // use command line arguments only in 1st iteration

            if (parameters.ContainsKey("method") && parameters["method"].ToLower() == "cikis") // if user enters "exit", do so
                break;

            SystemFunctions.ExecuteRelevantMethod(parameters); // execute desired method
        }

        return;
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register DbContext to service container, utilizing DI
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql("Server=localhost; Port=5432; Database=ArchiveGuardian; User Id=postgres; Password=123456AaAa; CommandTimeout=20; Timeout=15;"));
            }).ConfigureLogging((context, logging) => {
                var env = context.HostingEnvironment;
                var config = context.Configuration.GetSection("Logging");
                // ...
                logging.AddConfiguration(config);
                logging.AddConsole();
                // ...
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            });
}
