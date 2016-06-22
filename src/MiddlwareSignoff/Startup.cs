using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace MiddlwareSignoff
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

        public void Configure(IApplicationBuilder app, ILoggerFactory loggers )
        {
            loggers.AddConsole(LogLevel.Trace);
            _info.Invoke(this, new[] { app });
        }

        public void Configure_RequestProperties(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                await c.Response.WriteAsync(@"
<form method=post enctype=multipart/form-data>
<input type=text name=a></input>
<input type=file name=file></input>
<input type=submit></input>
</form>
");
                Dump(c.Response.Body, c);
            });
        }

        public void Configure_MutatingConnectionInfo(IApplicationBuilder app)
        {
            app.Use(async (c, next) =>
            {
                c.Connection.RemoteIpAddress = new System.Net.IPAddress(0);
                await next();
            });
            app.Run(async c =>
            {
                await Dump(c.Response.Body, c.Connection);
            });
        }

        public void Configure_ResonseProperties(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                c.Response.StatusCode = 1;
                c.Response.ContentLength = 1;
                c.Response.ContentType = "";
                await Dump(c.Response.Body, c.Response);
            });
        }

        public void Configure_ResonseHeaders(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                c.Response.Headers["CUSTOM"] += 1;
                await Dump(c.Response.Body, c.Response);
                c.Response.Headers["CUSTOM"] += 1;
            });
        }

        public void Configure_ResonseStatus(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                c.Response.StatusCode += 1;
                await Dump(c.Response.Body, c.Response);
                c.Response.StatusCode += 1;
            });
        }
        public void Configure_ResponseFile(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                //c.Response.ContentType="text/plain";
                await Dump(c.Response.Body, c.Response);
                await c.Response.SendFileAsync(app.ApplicationServices.GetService<IHostingEnvironment>().ContentRootFileProvider.GetFileInfo("web.config"));

            });
        }

        public void Configure_Callbacks(IApplicationBuilder app)
        {
            app.Use(async (c, next) =>
            {
                c.Response.OnStarting(async () => await c.Response.WriteAsync("OnStarting"));
                c.Response.OnCompleted(async () => await c.Response.WriteAsync("OnCompleted"));
                await next();
            });
            app.Run(async c =>
            {
                await Dump(c.Response.Body, c.Response);

                c.Response.OnStarting(async () => await c.Response.WriteAsync("OnStarting2"));
                c.Response.OnCompleted(async () => await c.Response.WriteAsync("OnCompleted2"));
            });
        }

        public void Configure_Redirects(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                if (c.Request.Path.Value == "/")
                {
                    c.Response.Redirect("http://localhost:5000/1");
                }
                await Dump(c.Response.Body, c.Response);
            });
        }
        public void Configure_Abort(IApplicationBuilder app)
        {
            app.Use(async (c, next) =>
            {
                await Task.WhenAny(next(), Task.Delay(1000));
                c.Abort();
            });
            app.Run(async c =>
            {
                while (!c.RequestAborted.IsCancellationRequested)
                {
                    await c.Response.WriteAsync("A! ");
                }
            });
        }

        public void Configure_Items(IApplicationBuilder app)
        {
            app.Use(async (c, next) =>
            {
                c.Items[0] = 0;
                await next();
            });
            app.Run(async c =>
            {
                await c.Response.WriteAsync(Convert.ToString(c.Items[0]));
            });
        }

        public void Configure_Session(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                await Dump(c.Response.Body, c.Session);
            });
        }

        public void Configure_WebSocket(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                await Dump(c.Response.Body, c.WebSockets);
            });
        }

        public void Configure_Map(IApplicationBuilder app)
        {
            app.Map("/req", a => a.Run(async c => await Dump(c.Response.Body, c.Request)));
            app.Map("/resp", a => a.Run(async c => await Dump(c.Response.Body, c.Response)));
            app.Map("/ctx", a => a.Run(async c => await Dump(c.Response.Body, c)));
        }

        public void Configure_Map2(IApplicationBuilder app)
        {
            app.MapWhen(c => c.Request.Query["k"] == "1", a => a.Run(async c => await Dump(c.Response.Body, c.Request)));
            app.MapWhen(c => c.Request.Query["k"] == "2", a => a.Run(async c => await Dump(c.Response.Body, c.Response)));
            app.MapWhen(c => c.Request.Query["k"] == "3", a => a.Run(async c => await Dump(c.Response.Body, c)));
        }

        public void Configure_MapMapMap(IApplicationBuilder app)
        {
            app.Map("/a", 
                c => {
                    c.Map("/b", b => 
                    {
                        b.Map("/c", bb => bb.Run(async cc => await cc.Response.WriteAsync("Yay")));
                        b.Run(async cc => await cc.Response.WriteAsync("Nope /a/b"));
                    });
                    c.Map("/c", b => b.Run(async cc => await cc.Response.WriteAsync("Nope /a/c")));
                    c.Run(async cc => await cc.Response.WriteAsync("Nope /a"));
                });

            app.Run(async c => await c.Response.WriteAsync("Nope"));
        }

        public void Configure_CustomMiddlware(IApplicationBuilder app)
        {
            app.UseMiddleware<CustomMiddlware>("string", 1);
            app.Run(async cc => await cc.Response.WriteAsync("Yay"));
        }

        public class CustomMiddlware
        {
            private int _i;
            private RequestDelegate _next;
            private string _s;

            public CustomMiddlware(RequestDelegate next, string s, int i)
            {
                _s = s;
                _i = i;
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                await context.Response.WriteAsync($"CustomMiddlware {_s} {_i}\r\n");
                await _next(context);
            }
        }

        private async Task Dump(Stream s, object o)
        {
            using (var writer = new StreamWriter(s, new UTF8Encoding(false), 1000, true))
            {
                await writer.WriteLineAsync("<pre>");
                await DumpValue(writer, o, new Dictionary<object, string>());
                await writer.WriteLineAsync("</pre>");
            }
        }
            
        private async Task DumpValue(StreamWriter b, object o, Dictionary<object, string> done, int ident = 0)
        {
            var i = new string(' ', ident * 2);
            if (o == null)
            {
                await b.WriteAsync(i);
                await b.WriteLineAsync("<null>");
                return;
            }

            var type = o.GetType();
            var info = type.GetTypeInfo();
            if (info.IsPrimitive
                || type.Namespace.StartsWith("System"))
            {
                await b.WriteLineAsync(o.ToString());
                return;
            }

            if (done.ContainsKey(o))
            {
                await b.WriteLineAsync($"<a href=#{done[o]}>{done[o]}</a>");
                return;
            }

            done.Add(o, (done.Count + 1).ToString());
            await b.WriteLineAsync($"<span id={done[o]}></span>");

            bool w = false;
            if (o is IEnumerable)
            {
                var j = 0;
                foreach (var item in (IEnumerable)o)
                {
                    w = true;
                    await b.WriteAsync(i);
                    await b.WriteAsync($"[{j++}] : ");
                    await DumpValue(b, item, done, ident + 1);
                 
                }
                if (!w) b.WriteLine();
            }
            var props = type.GetProperties();

            w = false;
            foreach (var p in props)
            {
                if (p.GetIndexParameters().Any() ||
                    p.DeclaringType.Namespace.StartsWith("System"))
                {
                    continue;
                }

                object value;
                try
                {
                    value = p.GetValue(o);
                }
                catch (Exception ex)
                {
                    value = ex.Message;
                }
                w = true;

                await b.WriteAsync(i);
                await b.WriteAsync($"{p.Name}: ");
                await DumpValue(b, value, done, ident+1);
            }
            if (!w)
            {
                await b.WriteLineAsync();
            }
        }
    }
}
