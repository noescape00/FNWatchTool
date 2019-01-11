using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WatchTool.Server.Dashboard
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }




        //public static IWebHost Initialize(IEnumerable<ServiceDescriptor> services, IWebHostBuilder webHostBuilder)
        //{
        //    webHostBuilder
        //        .UseKestrel(options =>
        //        {
        //            Action<ListenOptions> configureListener = listenOptions => { listenOptions.UseHttps(certificate); };
        //            var ipAddresses = Dns.GetHostAddresses(apiSettings.ApiUri.DnsSafeHost);
        //            foreach (var ipAddress in ipAddresses)
        //            {
        //                options.Listen(ipAddress, apiSettings.ApiPort, configureListener);
        //            }
        //        })
        //        .UseContentRoot(Directory.GetCurrentDirectory())
        //        .UseIISIntegration()
        //        .UseUrls(apiUri.ToString())
        //        .ConfigureServices(collection =>
        //        {
        //            if (services == null)
        //            {
        //                return;
        //            }
        //
        //            // copies all the services defined for the full node to the Api.
        //            // also copies over singleton instances already defined
        //            foreach (ServiceDescriptor service in services)
        //            {
        //                object obj = fullNode.Services.ServiceProvider.GetService(service.ServiceType);
        //                if (obj != null && service.Lifetime == ServiceLifetime.Singleton && service.ImplementationInstance == null)
        //                {
        //                    collection.AddSingleton(service.ServiceType, obj);
        //                }
        //                else
        //                {
        //                    collection.Add(service);
        //                }
        //            }
        //        })
        //        .UseStartup<Startup>();
        //
        //    IWebHost host = webHostBuilder.Build();
        //
        //    host.Start();
        //
        //    return host;
        //}
    }
}
