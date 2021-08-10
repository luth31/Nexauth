using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nexauth.Server {
    class Program {
        static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.SuppressStatusMessages(true);
                    webBuilder.UseKestrel(options => {
                        options.Listen(IPAddress.Any, 80);
                        options.Listen(IPAddress.Any, 443, listenOptions => {
                            listenOptions.UseHttps("localhost.pfx");
                        });
                    });
                    webBuilder.ConfigureLogging(logging => {
                        logging.AddFilter("Microsoft", LogLevel.Warning);
                        logging.AddFilter("System", LogLevel.Warning);
                    });
                });
    }
}
