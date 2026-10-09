using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
// Registrado ANTES de quem o consome so por legibilidade -- a ordem nao
// importa para o container. ProgressoService e ProvaService dependem dele.
builder.Services.AddScoped<ConclusaoCursoService>();
builder.Services.AddScoped<ProgressoService>();
builder.Services.AddScoped<EstudanteService>();
builder.Services.AddScoped<ContaService>();
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
// Com a API publicada no Azure, a origem que o navegador envia continua sendo
// a do CLIENTE, nao a do servidor: a aplicacao web rodando em localhost:8080
// chamando a API na nuvem e uma requisicao de origem cruzada, e precisa estar
// nesta lista.
//
// As origens de desenvolvimento ficam permitidas tambem em producao, por
// decisao consciente: web, mobile e desktop sao desenvolvidos localmente
// contra a API ja publicada. Liberar a origem NAO concede acesso a dado
// nenhum — todo endpoint exige token JWT, e o CORS controla qual pagina pode
// ler a resposta, nao quem pode autenticar.
//
// Origens adicionais (a aplicacao web depois de publicada) entram por
// configuracao, sem recompilar: no portal do Azure, em Configuracoes do
// App Service, criar as chaves
//     Cors__Origens__0 = https://techquest-web.azurestaticapps.net
//     Cors__Origens__1 = https://outro-dominio
// O dois sublinhados e como o Azure representa hierarquia de configuracao.
const string PoliticaCors = "FrontEndTechQuest";

var origensLocais = new[]
{
    "http://localhost:8080",
    "http://127.0.0.1:8080",
    "http://localhost:5500",      // Live Server do VS Code
    "http://127.0.0.1:5500",
    "http://localhost:5501",      // porta alternativa quando a 5500 esta ocupada
    "http://127.0.0.1:5501"
};

var origensPublicadas = builder.Configuration
    .GetSection("Cors:Origens")
    .Get<string[]>() ?? Array.Empty<string>();

var origensPermitidas = origensLocais
    .Concat(origensPublicadas)
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .Select(o => o.TrimEnd('/'))
    .Distinct()
    .ToArray();

builder.Services.AddCors(opt =>
    opt.AddPolicy(PoliticaCors, p => p
        .WithOrigins(origensPermitidas)
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

// Swagger ligado tambem em producao, de proposito: e a documentacao viva do
// contrato da API, consultada pelos clientes web, mobile e desktop durante o
// desenvolvimento e usada na demonstracao da banca. O que continua restrito a
// Development e o DETALHE TECNICO DO ERRO (stack trace), tratado no
// TratamentoDeErrosMiddleware — essa sim e a informacao que nao pode vazar.
//
// ALTERNATIVA REJEITADA: definir ASPNETCORE_ENVIRONMENT=Development no App
// Service para o Swagger aparecer. Funcionaria, mas ligaria junto a exposicao
// de stack trace e desligaria otimizacoes de producao — resolveria a
// documentacao criando um problema maior.
app.UseSwagger();
app.UseSwaggerUI();

// No App Service o TLS termina na borda do Azure, que repassa a requisicao
// para a aplicacao. Sem esta linha o ASP.NET Core enxerga a requisicao
// interna como HTTP e o UseHttpsRedirection abaixo entraria em laco de
// redirecionamento. O cabecalho X-Forwarded-Proto preserva o esquema original.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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
