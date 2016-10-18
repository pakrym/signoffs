using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Compression
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/slow", a =>
            {
                a.UseResponseCompression();
                a.Run(async w =>
                {
                    w.Response.Headers["Content-Type"] = "text/html";

                    while (!w.RequestAborted.IsCancellationRequested)
                    {
                        await w.Response.WriteAsync("1");
                        await w.Response.Body.FlushAsync();
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                    }
                });
            });

            app.Map("/slownb", a =>
            {
                a.UseResponseCompression();
                a.Run(async w =>
                {
                    w.Response.Headers["Content-Type"] = "text/html";

                    var nb = w.Features.Get<IHttpBufferingFeature>();
                    nb.DisableResponseBuffering();

                    while (!w.RequestAborted.IsCancellationRequested)
                    {
                        await w.Response.WriteAsync("1");
                        await w.Response.Body.FlushAsync();
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                    }
                });
            });
            app.Map("/slownc", a =>
            {
                a.Run(async w =>
                {
                    w.Response.Headers["Content-Type"] = "text/html";

                    var nb = w.Features.Get<IHttpBufferingFeature>();
                    nb?.DisableResponseBuffering();

                    while (!w.RequestAborted.IsCancellationRequested)
                    {
                        await w.Response.WriteAsync("1");
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                    }
                });
            });

            app.Map("/buff", a =>
            {
                a.UseResponseBuffering();
                a.Run(async w =>
                {
                    w.Response.Headers["Content-Type"] = "text/html";

                    while (!w.RequestAborted.IsCancellationRequested)
                    {
                        await w.Response.WriteAsync("1");
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                    }
                });
            });
            app.Map("/buffnb", a =>
            {
                a.UseResponseBuffering();
                a.Run(async w =>
                {
                    w.Response.Headers["Content-Type"] = "text/html";

                    var nb = w.Features.Get<IHttpBufferingFeature>();
                    nb?.DisableResponseBuffering();

                    while (!w.RequestAborted.IsCancellationRequested)
                    {
                        await w.Response.WriteAsync("1");
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                    }
                });
            });

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                context.Response.Headers["Content-Type"] = "text/html";
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
