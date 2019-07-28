﻿using System;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json.Serialization;

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
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();
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
                //setUpAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

            })
            .AddJsonOptions(options => {
                options.SerializerSettings.ContractResolver = 
                new CamelCasePropertyNamesContractResolver();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Disable automatic model state validation before comes into the controller
            services.Configure<ApiBehaviorOptions>(options => 
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // TODO: Update the loging functionality
            //loggerFactory.AddDebug(LogLevel.Information);
            //loggerFactory.AddProvider(new NLogLoggerProvider());
            //loggerFactory.AddNLog();
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
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500,exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }
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
