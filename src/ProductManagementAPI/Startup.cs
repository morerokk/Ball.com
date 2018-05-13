﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pitstop.Infrastructure.Messaging;
using ProductManagementAPI.Database;
using ProductManagementAPI.Infrastructure.Database;
using ProductManagementAPI.Repositories;

namespace ProductManagementAPI
{
    public class Startup
    {
	    public Startup(IHostingEnvironment env)
	    {
		    var builder = new ConfigurationBuilder()
			    .SetBasePath(env.ContentRootPath)
			    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
			    .AddEnvironmentVariables();
		    Configuration = builder.Build();
	    }

		public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

			// add DBContext classes
			var sqlConnectionString = Configuration.GetConnectionString("ProductManagementCN");
			services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(sqlConnectionString));

			// add messagepublisher classes
			var configSection = Configuration.GetSection("RabbitMQ");
	        string host = configSection["Host"];
	        string userName = configSection["UserName"];
	        string password = configSection["Password"];
	        services.AddTransient<IMessagePublisher>((sp) => new RabbitMQMessagePublisher(host, userName, password, "Pitstop"));
	        services.AddTransient<IProductRepository, EFProductRepository>();

			services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ProductDbContext _context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

			ProductDbSeeder.Seed(_context);
        }
    }
}
