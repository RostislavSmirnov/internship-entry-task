using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Logging; 
using System;
using System.Linq;
using X0Game.Data;
using X0Game.Interfaices;
using X0Game.Repositories;
using X0Game.Services;

namespace X0GamesIntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
                if (dbContextOptionsDescriptor != null) { services.Remove(dbContextOptionsDescriptor); }

                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(GameDbContext));
                if (dbContextDescriptor != null) { services.Remove(dbContextDescriptor); }

                var repoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGameRepository));
                if (repoDescriptor != null) { services.Remove(repoDescriptor); }

                var serviceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGameSerice));
                if (serviceDescriptor != null) { services.Remove(serviceDescriptor); }

            
                services.AddDbContext<GameDbContext>(options =>
                {
                    var dbName = "InMemoryDb_" + Guid.NewGuid();
                    options.UseInMemoryDatabase(dbName);
                });

                services.AddScoped<IGameRepository, GameRepository>();
                services.AddScoped<IGameSerice, GameService>();
            });


            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); 
                logging.AddConsole();     
            });
        }

        
        protected override IHost CreateHost(IHostBuilder builder)
        {
            try
            {
                return base.CreateHost(builder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!! HOST CREATION FAILED !!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                throw;
            }
        }
    }
}
