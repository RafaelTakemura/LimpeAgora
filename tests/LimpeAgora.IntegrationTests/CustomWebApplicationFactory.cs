using LimpeAgoraApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;

namespace LimpeAgora.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mysqlContainer;
    private bool _useDocker = false;

    public async Task InitializeAsync()
    {
        try
        {
            _mysqlContainer = new MySqlBuilder("mysql:8.0")
                .WithDatabase("limpeagora_test_db")
                .WithUsername("root")
                .WithPassword("root_test_pass")
                .Build();

            await _mysqlContainer.StartAsync();
            _useDocker = true;
        }
        catch
        {
            _useDocker = false;
            _mysqlContainer = null;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove os descritores de configuração do DbContext anterior
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<LimpeAgoraDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(LimpeAgoraDbContext)).ToList();

            foreach (var d in descriptors)
            {
                services.Remove(d);
            }

            if (_useDocker && _mysqlContainer != null)
            {
                var connectionString = _mysqlContainer.GetConnectionString();
                services.AddDbContext<LimpeAgoraDbContext>(options =>
                {
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
                });
            }
            else
            {
                // Isola o provedor InMemory com seu próprio ServiceProvider interno para evitar conflito com Pomelo
                var inMemoryInternalProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<LimpeAgoraDbContext>(options =>
                {
                    options.UseInMemoryDatabase("LimpeAgoraIntegrationTestDb")
                           .UseInternalServiceProvider(inMemoryInternalProvider)
                           .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            }

            // Inicializa o banco de testes
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LimpeAgoraDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public new async Task DisposeAsync()
    {
        if (_mysqlContainer != null)
        {
            await _mysqlContainer.DisposeAsync();
        }
    }
}
