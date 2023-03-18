using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SingleBlog.Entities;
using System.IO;

namespace SingleBlog
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
            services.AddControllers();
            services.AddDbContext<SingleBlogDBContext>(options => options.UseSqlite($"Data Source={PathUtils.DbFilePath}"));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SingleBlog", Version = "v1" });
                c.OperationFilter<MySwaggerHeaderFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable swagger for DEMO reasons also if it is't  in Development
            //if (env.IsDevelopment())
            //{
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SingleBlog v1"));
            //}

            // Disable Https redirection for DEMO
            //app.UseHttpsRedirection();
            
            //Create folder for images if not exist
            var imageFsDir = PathUtils.ImagesContentRootPath;
            if (!Directory.Exists(imageFsDir))
                Directory.CreateDirectory(imageFsDir);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
