using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechQuest.API.Middlewares;
using TechQuest.Application.Interfaces;
using TechQuest.Application.Servicos;
using TechQuest.Infrastructure.Persistencia;
using TechQuest.Infrastructure.Persistencia.Repositorios;
using TechQuest.Infrastructure.Seguranca;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. BANCO DE DADOS
// ---------------------------------------------------------------------------
// EnableRetryOnFailure e obrigatorio no Azure SQL: o tier serverless faz
// auto-pause, e a primeira consulta depois da pausa falha por timeout.
builder.Services.AddDbContext<TechQuestDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("TechQuestDB"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// ---------------------------------------------------------------------------
// 2. INJECAO DE DEPENDENCIA
// ---------------------------------------------------------------------------
// Unico ponto do sistema que conhece Application e Infrastructure ao mesmo
// tempo: e aqui que cada contrato recebe sua implementacao concreta.
builder.Services.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<IProvaRepository, ProvaRepository>();
builder.Services.AddScoped<IDesempenhoRepository, DesempenhoRepository>();
builder.Services.AddScoped<IGamificacaoRepository, GamificacaoRepository>();
builder.Services.AddScoped<IProgressoRepository, ProgressoRepository>();
builder.Services.AddScoped<IHistoricoRepository, HistoricoRepository>();
builder.Services.AddScoped<ICertificadoRepository, CertificadoRepository>();
builder.Services.AddScoped<IConquistaRepository, ConquistaRepository>();
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddSingleton<IHashSenhaService, BCryptHashService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<CursoService>();
builder.Services.AddScoped<ProvaService>();
builder.Services.AddScoped<GamificacaoService>();
builder.Services.AddScoped<ConquistaService>();
builder.Services.AddScoped<ProgressoService>();
builder.Services.AddScoped<EstudanteService>();
builder.Services.AddScoped<ChamadoService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<TutorService>();
builder.Services.AddScoped<AdminService>();

// ---------------------------------------------------------------------------
// 3. AUTENTICACAO JWT
// ---------------------------------------------------------------------------
var chaveJwt = builder.Configuration["Jwt:Chave"]
    ?? throw new InvalidOperationException(
        "Jwt:Chave nao configurada. Use os User Secrets do Visual Studio.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// 4. CORS
// ---------------------------------------------------------------------------
const string PoliticaCors = "FrontEndTechQuest";
builder.Services.AddCors(opt =>
    opt.AddPolicy(PoliticaCors, p => p
        .WithOrigins(
            "http://localhost:8080",
            "http://127.0.0.1:8080",
            "http://localhost:5500")   // Live Server do VS Code
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ---------------------------------------------------------------------------
// 5. CONTROLLERS E SWAGGER
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tech Quest API",
        Version = "v1",
        Description = "API REST da plataforma Tech Quest - PIM IV / ADS - UNIP"
    });

    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token, sem o prefixo Bearer.",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, esquema);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = Array.Empty<string>() });
});

var app = builder.Build();

// PRIMEIRO da fila de propósito: para capturar a exceção de qualquer
// middleware ou controller que venha depois dele, o tratamento de erros
// precisa envolver todo o resto do pipeline.
app.UseMiddleware<TratamentoDeErrosMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(PoliticaCors);

// A ordem importa: autenticar (quem e) antes de autorizar (pode o que).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", async (TechQuestDbContext db) =>
{
    var conectado = await db.Database.CanConnectAsync();
    return conectado
        ? Results.Ok(new { status = "ok", banco = "conectado" })
        : Results.Problem("Banco inacessivel");
});

app.Run();
