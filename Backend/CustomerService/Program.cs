using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CustomerService.Repositories;
using CustomerService.Models;

namespace CustomerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // If you are using credentials (like cookies or tokens)
                });
            });

            // Database connection string
            builder.Services.AddDbContext<CustomerBankContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyDBConnection")));

            // JWT Configuration from appsettings.json
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                });

            // Add Authorization services
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Default", policy => policy.RequireAuthenticatedUser());
            });

            // Register repositories
            // Register repositories
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<CustomerRepository>(); // Add this line
            builder.Services.AddScoped<EmailService>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var smtpSettings = configuration.GetSection("SmtpSettings");

                var smtpServer = smtpSettings["Server"];
                var port = int.Parse(smtpSettings["Port"]);
                var email = smtpSettings["Username"];
                var password = smtpSettings["Password"];

                return new EmailService(smtpServer, port, email, password);
            });

            // Register AuthService with dependencies
            builder.Services.AddScoped<AuthService>(provider =>
            {
                var customerRepo = provider.GetRequiredService<ICustomerRepository>();
                return new AuthService(customerRepo, key, issuer, audience);
            });
            builder.Services.AddSingleton<SmsService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new SmsService(
                    config["Twilio:AccountSid"],
                    config["Twilio:AuthToken"],
                    config["Twilio:FromPhoneNumber"]);
            });
            builder.Services.AddScoped<OtpService>();

            // Add controllers
            builder.Services.AddControllers(); // Make sure to add this line

            // Swagger configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Customer API",
                    Version = "v1"
                });

                // Add security definition for JWT
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Enable Swagger for development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowAngularApp");
            app.UseHttpsRedirection();

            // Enable authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); // This maps the controller routes

            app.Run();
        }
    }
}