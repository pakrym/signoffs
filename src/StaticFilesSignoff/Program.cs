using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace StaticFilesSignoff
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                var lifetime = (IApplicationLifetime)host.Services.GetService(typeof(IApplicationLifetime));
                Task.Run(() =>
                {

                    while (true)
                    {
                        var line = Console.ReadLine();
                        if (line == "r")
                        {
                            Console.WriteLine("Restarting");
                            lifetime.StopApplication();
                            break;
                        }
                    }
                });

                host.Run();
            }
            
        }
    }
}
