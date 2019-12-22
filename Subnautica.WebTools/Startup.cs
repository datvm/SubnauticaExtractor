using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibliTech.VersionedFileProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Subnautica.WebTools
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            var versionedFileProvider = new VersionedFileProvider(env.WebRootPath);
            var mixedFileProvider = new CompositeFileProvider(versionedFileProvider, env.WebRootFileProvider);
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = mixedFileProvider,
            });
            env.WebRootFileProvider = mixedFileProvider;

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
