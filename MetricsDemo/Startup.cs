using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using App.Metrics;
using App.Metrics.Counter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MetricsDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMetrics(Program.Metrics);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                AutoDiscoverRoutes(context);
                // Do logging or other work that doesn't write to the Response.
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void AutoDiscoverRoutes(HttpContext context)
        {
            if (context.Request.Path.Value == "/favicon.ico")
                return;

            List<string> keys = new List<string>();
            List<string> vals = new List<string>();

            var routeData = context.GetRouteData();
            if (routeData != null)
            {
                keys.AddRange(routeData.Values.Keys);
                vals.AddRange(routeData.Values.Values.Select(p => p.ToString()));
            }

            keys.Add("method"); vals.Add(context.Request.Method);
            keys.Add("response"); vals.Add(context.Response.StatusCode.ToString());
            keys.Add("url"); vals.Add(context.Request.Path.Value);

            Program.Metrics.Measure.Counter.Increment(new CounterOptions
            {
                Name = "api",
                //ResetOnReporting = true,
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags(keys.ToArray(), vals.ToArray())
            });
        }
    }
}