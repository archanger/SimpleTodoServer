using System;
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
using Microsoft.AspNetCore.WebSockets;
using TodoAPI.Core;
using TodoAPI.Core.Models;
using TodoAPI.Persistance;
using AutoMapper;
using TodoAPI.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using SimpleTodoServer.Filters;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading;
using TodoAPI.Middleware;
using TodoAPI.Chat;
using TodoAPI.Helpers;

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
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.Configure<HostEmailSettings>(Configuration.GetSection("EmailHosting"));
            services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));
            services.AddDbContext<TodoContext>( opt => opt.UseSqlServer(Configuration.GetConnectionString("Default")));
            
            var jwtSettings = Configuration.GetSection("Jwt").Get<JwtSettings>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = JwtHelper.GetJwtParamteres(jwtSettings);
            });
            
            services.AddAutoMapper();
            services.AddMvc(options => {
                options.Filters.Add(new ValidationFilter());
            });
            services.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions 
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            });
            app.UseAuthentication();
            app.UseMvc();

            app.UseCustomWebSocket( router => {
                router.MapHub<ChatRoom>("/public");
            });
        }
    }
}
