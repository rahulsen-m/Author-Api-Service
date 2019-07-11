using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace Library.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            //Added the connection string
            // services.AddDbContext<LibraryContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:LibraryContext"]));
            // services.AddDbContext<LibraryContext>(options =>
            // options.UseSqlServer(Configuration.GetConnectionString("LibraryContext")));
            services.AddDbContext<LibraryContext>(options =>
            options.UseSqlServer("Server=RAHUL-PC;Database=Author_Api;Trusted_Connection=True;MultipleActiveResultSets=true;"));
            // services.AddDbContext<LibraryContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LibraryContext")));
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            // Set configuration to accept json or xml request type and return the response in the same type
            services.AddMvc(setUpAction => 
            {   
                //Default is false, which means this support all request type and return only in default type(json)
                // Setting true we can force that to return the response in the specified type(XML)
                setUpAction.ReturnHttpNotAcceptable = true;
                // Setting output formatter
                setUpAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                // Setting input formatter
                setUpAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Disable automatic model state validation before comes into the controller
            services.Configure<ApiBehaviorOptions>(options => 
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {                
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                // This configuration will write the error message in the response
                // Global exception handler for the status code 500
                app.UseExceptionHandler(appBuilder => 
                {
                    // Adding action to the context
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happend. Please try again.");
                    });
                });
            }          

            app.UseHttpsRedirection();
            app.UseMvc();
            DbInitializer.EnsureSeedDataForContext(app);
        }
    }
}
