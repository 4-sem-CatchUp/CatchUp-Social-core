using Microsoft.EntityFrameworkCore;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Middleware;

namespace Social
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add logging config
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

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

            // Middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>(); // Catches all errors
            app.UseMiddleware<RequestLoggingMiddleware>(); // Logging all successes

            app.MapControllers();

            app.Run();
        }
    }
}
