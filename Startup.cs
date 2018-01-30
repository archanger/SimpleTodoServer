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
using TodoAPI.Core;
using TodoAPI.Core.Models;
using TodoAPI.Persistance;
using AutoMapper;
using TodoAPI.Config;
using Microsoft.AspNetCore.Identity;

namespace TodoAPI
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
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<TodoContext>( opt => opt.UseSqlServer(Configuration.GetConnectionString("Default")));
            // Identity
            services.AddDbContext<UserDbContext>(opt => opt.UseInMemoryDatabase("accounts"));
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<UserDbContext>();
            // Identity
            services.AddAutoMapper();
            services.AddMvc();
            services.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
    
            app.UseMvc();
        }
    }
}
