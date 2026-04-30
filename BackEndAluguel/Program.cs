using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackEndAluguel.Api.Background;
using BackEndAluguel.Api.Middleware;
using BackEndAluguel.Application;
using BackEndAluguel.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===========================================================
// Registro de serviços — Clean Architecture
// ===========================================================

// Camada Application: registra todos os manipuladores CQRS (MediatR)
builder.Services.AdicionarApplication();

// Camada Infrastructure: registra DbContext (SQL Server), repositórios e serviços externos
builder.Services.AdicionarInfrastructure(builder.Configuration);

// Servico de background: verifica e atualiza faturas vencidas diariamente
builder.Services.AddHostedService<VerificarFaturasVencidasServico>();

// Autenticacao JWT — valida tokens emitidos pelo JwtServico
// Configuracoes lidas de appsettings.json -> secao "Jwt"
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
            ClockSkew = TimeSpan.Zero // Remove tolerancia de 5 min padrao
        };
    });

// Controllers com serialização JSON configurada:
// - camelCase nos campos (padrão REST/JavaScript)
// - Enums serializados como string legível (ex: "Pendente" em vez de 1)
// - DateOnly suportado nativamente
// - Ciclos de referência ignorados (evita loop Inquilino ↔ Fatura)
builder.Services.AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoes.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opcoes.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger/OpenAPI — documentação interativa dos endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new()
    {
        Title = "GestAluguel API",
        Version = "v1",
        Description = "API RESTful para gerenciamento de aluguéis residenciais. " +
                      "Gerencie apartamentos, inquilinos e faturas mensais."
    });

    // Inclui os comentários XML na documentação Swagger
    var xmlApi = Path.Combine(AppContext.BaseDirectory, "BackEndAluguel.xml");
    if (File.Exists(xmlApi))
        opcoes.IncludeXmlComments(xmlApi);

    // Suporte a JWT Bearer no Swagger UI
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS — permite que o front-end React acesse a API
// Em produção substitua a política pelo domínio real do front-end
builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirFrontEnd", politica =>
        politica
            .WithOrigins(
                "http://localhost:3000",   // React (Create React App / Vite padrão)
                "http://localhost:5173",   // Vite alternativo
                "http://localhost:4200"    // Angular (caso necessário)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// ===========================================================
// Pipeline de requisições HTTP
// ===========================================================

// Deve ser o PRIMEIRO middleware — captura todas as exceções não tratadas
app.UseTratamentoDeErros();

if (app.Environment.IsDevelopment())
{
    // Swagger UI disponível apenas em desenvolvimento: https://localhost:7200/swagger
    app.UseSwagger();
    app.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "GestAluguel API v1");
        opcoes.RoutePrefix = "swagger";
        opcoes.DocumentTitle = "GestAluguel API";
        opcoes.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

// Serve arquivos estaticos — necessario para download de contratos salvos em wwwroot
app.UseStaticFiles();

// CORS deve ser registrado antes de UseAuthorization e MapControllers
app.UseCors("PermitirFrontEnd");

// Autenticacao deve vir antes de Autorizacao
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
