using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Encodings.Web;

namespace StaticFilesSignoff
{
    public class Startup
    {
        private MethodInfo _info;

        private MethodInfo GetMethod(string prefix, out string sufix)
        {
            var methods = this.GetType().GetTypeInfo().GetMethods().Where(m => m.Name.StartsWith(prefix)).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                Console.WriteLine($"{i}. {methods[i].Name.Substring(prefix.Length)}");
            }
            var number = int.Parse(Console.ReadLine());
            sufix = methods[number].Name.Substring(prefix.Length);
            return methods[number];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string sufix;
            _info = GetMethod("Configure_", out sufix);
            var method = this.GetType().GetTypeInfo().GetMethod("ConfigureServices_" + sufix);
            if (method != null)
            {
                method.Invoke(this, new[] { services });
            }
        }


        public void Configure(IApplicationBuilder app)
        {
            _info.Invoke(this, new[] { app });
        }

        public void Configure_FileServer(IApplicationBuilder app)
        {
            app.UseFileServer();
        }

        public void Configure_StaticFiles(IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }

        public void Configure_StaticFilesDefault(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        public void Configure_StaticFilesHeaders(IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = c => c.Context.Response.Headers.Append("NAME", c.File.Name)
            });
        }
        public void Configure_StaticServeUnknownFileTypes(IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = true
            });
        }

        public void Configure_StaticFilesRequestPath(IApplicationBuilder app)
        {
            app.UseStaticFiles("/Public");
        }

        public void Configure_FileServerRequestPath(IApplicationBuilder app)
        {
            app.UseFileServer("/Public");
        }

        public void Configure_FileServerDirectoryBrowser(IApplicationBuilder app)
        {
            app.UseFileServer(true);
        }

        public void Configure_FileServerDirectoryBrowserNoDefaults(IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions()
            {
                EnableDefaultFiles = false,
                EnableDirectoryBrowsing = true
            });
        }

        public void Configure_FileServerEmbedded(IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly),
                EnableDirectoryBrowsing = true
            });
        }

        public void Configure_FileServerFormatter(IApplicationBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                Formatter = new Formatter()
            });
        }

        class Formatter : HtmlDirectoryFormatter
        {
            public Formatter() : base(HtmlEncoder.Default)
            {
            }

            public override async Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
            {
                contents = contents.Where(c => c.IsDirectory);
                await base.GenerateContentAsync(context, contents);
            }
        }

        public void Configure_FileServerEmbeddedDefaultFiles(IApplicationBuilder app)
        {
            var fs = new EmbeddedFileProvider(this.GetType().GetTypeInfo().Assembly, "StaticFilesSignoff.wwwroot");;
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                DefaultFileNames = new[] { "TextFile.txt" },
                FileProvider = fs
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = fs,
            });
        }
        
        public void ConfigureServices_FileServerFormatter(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }

        public void ConfigureServices_FileServerEmbedded(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }
        
        public void ConfigureServices_FileServerDirectoryBrowserNoDefaults(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }

        public void ConfigureServices_FileServerDirectoryBrowser(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }
    }
}
