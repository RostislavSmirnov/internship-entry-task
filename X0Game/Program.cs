using Serilog;
using Serilog.Sinks.File;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using X0Game.Data;
using X0Game.Interfaices;
using X0Game.Repositories;
using X0Game.Services;
using X0Game.ErrorHandler;

namespace X0Game
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<Data.GameDbContext>(options => options.UseNpgsql(connectionString));
             
                builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
                
                builder.Services.AddSwaggerGen();

                builder.Services.AddScoped<IGameRepository, GameRepository>();
                builder.Services.AddScoped<IGameSerice, GameService>();

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                
                WebApplication app = builder.Build();

                app.UseMiddleware<ErrorHandlerMiddleware>();
                
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
            catch (Exception WebApiException)
            {
                Log.Fatal(WebApiException, "Приложение было завершилось с ошибкой");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
