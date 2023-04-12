using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BaGet.Core;
using BaGet.Web;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaGet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            if (!host.ValidateStartupOptions())
            {
                return;
            }
            CerMaker.CreateIfNotExist();
            var app = new CommandLineApplication
            {
                Name = "baget",
                Description = "A light-weight NuGet service",
            };

            app.HelpOption(inherited: true);

            app.Command("import", import =>
            {
                import.Command("downloads", downloads =>
                {
                    downloads.OnExecuteAsync(async cancellationToken =>
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var importer = scope.ServiceProvider.GetRequiredService<DownloadsImporter>();

                            await importer.ImportAsync(cancellationToken);
                        }
                    });
                });
            });

            app.Option("--urls", "The URLs that BaGet should bind to.", CommandOptionType.SingleValue);

            app.OnExecuteAsync(async cancellationToken =>
            {
                await host.RunMigrationsAsync(cancellationToken);
                await host.RunAsync(cancellationToken);
            });

            await app.ExecuteAsync(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    var root = Environment.GetEnvironmentVariable("BAGET_CONFIG_ROOT");

                    if (!string.IsNullOrEmpty(root))
                    {
                        config.SetBasePath(root);
                    }
                })
                .ConfigureWebHostDefaults(web =>
                {
                    web.ConfigureKestrel(options =>
                    {
                        // Remove the upload limit from Kestrel. If needed, an upload limit can
                        // be enforced by a reverse proxy server, like IIS.
                        options.Limits.MaxRequestBodySize = null;
                        // options.ConfigureHttpsDefaults(it =>
                        // {
                        //     it.ServerCertificate = new X509Certificate2(CerMaker.CER_FILE, CerMaker.PASSWD);
                        // });
                        // options.ListenAnyIP(18443, options =>
                        // {
                        //     options.UseHttps(it =>
                        //     {
                        //         it.ServerCertificate = new X509Certificate2(CerMaker.CER_FILE, CerMaker.PASSWD);
                        //     });
                        // });
                    });
                    web.UseStartup<Startup>();
                });
        }


    }
}
