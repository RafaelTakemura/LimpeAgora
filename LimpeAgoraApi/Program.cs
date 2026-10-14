using System.Text;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Middlewares;
using LimpeAgoraApi.Services.Implementations;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuração do CORS para permitir consumo pelo Blazor Web e MAUI Mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuração do DbContext com MySQL via Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=limpeagora_dev;Uid=root;Pwd=root;";

builder.Services.AddDbContext<LimpeAgoraDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
});

// Configuração da Autenticação JWT
var chaveSecreta = builder.Configuration["Jwt:Key"] ?? "LimpeAgora_Super_Segredo_Chave_De_Desenvolvimento_2026_UdC_WebMobile!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "LimpeAgoraApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "LimpeAgoraClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Registro dos Serviços de Domínio e Infraestrutura
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

var app = builder.Build();

// Middleware Global de Tratamento de Exceções
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seeder de desenvolvimento para banco de dados e usuário administrador
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<LimpeAgoraDbContext>();
        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
        }

        var passwordService = services.GetRequiredService<IPasswordService>();
        if (!db.Usuarios.Any(u => u.Email == "admin@limpeagora.com"))
        {
            db.Usuarios.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                NomeCompleto = "Administrador do Sistema",
                Email = "admin@limpeagora.com",
                Telefone = "45999998888",
                SenhaHash = passwordService.HashPassword("Admin@123"),
                Perfil = PerfilUsuario.Administrador,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Aviso: Não foi possível aplicar migrations automáticas ou seed no banco.");
    }
}

app.Run();

public partial class Program { }
