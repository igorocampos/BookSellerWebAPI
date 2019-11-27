using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace BookSellerWebAPI
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
            services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
                    .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddDbContext<BookSellerContext>(options => options.UseNpgsql(Configuration.GetConnectionString("BookSellerContext")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<BookSellerContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"])),
                        ClockSkew = TimeSpan.Zero,
                    });

            // Register the Swagger generator
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("BookSellerWebAPI",
                                   new OpenApiInfo
                                   {
                                       Title = "Book Seller API",
                                       Version = "v1",
                                       Description = "This is an example where one can use to register Books, Authors and Reviews for a Book Seller website.",
                                       Contact = new OpenApiContact
                                       {
                                           Name = "Igor Campos",
                                           Email = "igorocampos@gmail.com",
                                           Url = new Uri("https://github.com/igorocampos/")
                                       }
                                   });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                //Please see in https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269
                //that this was marked as obsolete, however the alternative is not done yet.
#pragma warning disable CS0618 // Type or member is obsolete
                options.DescribeAllEnumsAsStrings();
#pragma warning restore CS0618 // Type or member is obsolete

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. <br><br>
                      <b>Please enter 'Bearer' [space] and then your token in the text input below.</b><br><br>
                      Example: <pre>Bearer 12345abcdef</pre><br><br>",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/BookSellerWebAPI/swagger.json", "API v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
