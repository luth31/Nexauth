using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace nexauth_server {
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
                        options.Listen(System.Net.IPAddress.Any, 80);
                    });
                    webBuilder.ConfigureLogging(logging => {
                        logging.ClearProviders();
                    });
                });
    }
}
