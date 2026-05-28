using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackEndAluguel.Api.Background;
using BackEndAluguel.Api.Middleware;
using BackEndAluguel.Application;
using BackEndAluguel.Infrastructure;
using BackEndAluguel.Infrastructure.Contexto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===========================================================
// Registro de serviços — Clean Architecture
// ===========================================================

builder.Services.AdicionarApplication();
builder.Services.AdicionarInfrastructure(builder.Configuration);
builder.Services.AddHostedService<VerificarFaturasVencidasServico>();

// Modo local: estado de heartbeat (singleton) e monitor que encerra o processo quando o browser fecha
builder.Services.AddSingleton<HeartbeatEstado>();
builder.Services.AddHostedService<HeartbeatMonitorServico>();

var jwtChave = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey nao configurado no appsettings.json.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GestAluguelAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GestAluguelFrontEnd";

builder.Services
    .AddAuthentication(opcoes =>
    {
        opcoes.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opcoes.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtChave)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoes.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opcoes.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new()
    {
        Title = "GestAluguel API",
        Version = "v1",
        Description = "API RESTful para gerenciamento de aluguéis residenciais."
    });

    var xmlApi = Path.Combine(AppContext.BaseDirectory, "BackEndAluguel.xml");
    if (File.Exists(xmlApi))
        opcoes.IncludeXmlComments(xmlApi);

    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {seu_token}"
    });

    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS permissivo para modo local (frontend servido pelo próprio backend)
builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirFrontEnd", politica =>
        politica
            .SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// ===========================================================
// Migração automática do banco SQLite na inicialização
// ===========================================================
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AluguelDbContext>();
    await db.Database.MigrateAsync();
}

// ===========================================================
// Pipeline de requisições HTTP
// ===========================================================

app.UseTratamentoDeErros();

app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "GestAluguel API v1");
    opcoes.RoutePrefix = "swagger";
    opcoes.DocumentTitle = "GestAluguel API";
    opcoes.DisplayRequestDuration();
});

// Serve arquivos estáticos do frontend (wwwroot/) e contratos
var provedor = new FileExtensionContentTypeProvider();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provedor,
    ServeUnknownFileTypes = false
});

app.UseCors("PermitirFrontEnd");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Fallback SPA: rotas não-API retornam o index.html do frontend
app.MapFallbackToFile("index.html");

app.Run();
