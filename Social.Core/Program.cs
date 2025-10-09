using Microsoft.EntityFrameworkCore;
using Social.Infrastructure.Persistens.dbContexts;

namespace Social
{
    public class Program
    {
        /// <summary>
        /// Builds and runs the web application: configures services (SignalR, MVC controllers, Swagger/OpenAPI, and SocialDbContext using SQL Server) and sets up the HTTP request pipeline.
        /// </summary>
        /// <param name="args">Command-line arguments forwarded to the web application builder.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSignalR();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<SocialDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SocialDb"))
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}