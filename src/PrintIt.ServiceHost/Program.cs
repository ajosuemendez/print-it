using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrintIt.Core.Pdfium;

namespace PrintIt.ServiceHost
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {


        public static void Main(string[] args)
        {
            PdfLibrary.EnsureInitialized();

            // For some reason the .env cant be found cause when we are starting in another folder when running dotnet run
            string currentDirectory = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(currentDirectory, ".env")))
            {
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                
                // If we've reached the root directory and still haven't found the .env file
                if (currentDirectory == null)
                {
                    throw new FileNotFoundException("Could not find the .env file.");
                }
            }

            string envPath = Path.Combine(currentDirectory, ".env");
            DotNetEnv.Env.Load(envPath);
            

            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            IWebHost host = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray(), isService).Build();

            if (isService)
            {
                using var customWebHostService = new CustomWebHostService(host);
                ServiceBase.Run(customWebHostService);
            }
            else
            {
                host.Run();
            }
        }
        private static IWebHostBuilder CreateWebHostBuilder(string[] args, bool isService)
        {
            
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging
                        .ClearProviders()
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("Microsoft.AspNetCore.Mvc.Internal", LogLevel.Warning)
                        .AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddConsole();

                    if (isService)
                        logging.AddEventLog(settings =>
                        {
                            settings.SourceName = "PrintIt";
                        });
                })
                .UseStartup<Startup>()
                // .UseUrls(configuration.GetValue<string>("Host:Urls"))
                .UseUrls(Environment.GetEnvironmentVariable("HOST") + Environment.GetEnvironmentVariable("PORT"))
                .UseKestrel();
                // .UseConfiguration(configuration);
        }
    }
}
