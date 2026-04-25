using ApiProcessamento.Config;
using ApiProcessamento.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Configurações da API ──────────────────────────────────────────────────────
builder.Services.Configure<ApiConfig>(
    builder.Configuration.GetSection("ApiConfig"));

// ── Banco de dados SQLite ─────────────────────────────────────────────────────
builder.Services.AddDbContext<SensorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=simi_sensores.db"));

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger com documentação XML ──────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIMI — Sistema Integrado de Monitoramento Industrial",
        Version = "v1",
        Description = """
            API responsável pelo recebimento e persistência de dados de sensores industriais.
            
            ## Sinais monitorados
            - **Temperatura** (°C) — monitoramento térmico de processos
            - **Pressão** (bar) — controle de sistemas pneumáticos e hidráulicos
            - **Umidade** (%) — qualidade ambiental e processos sensíveis à umidade
            - **Vibração** (m/s²) — detecção de falhas mecânicas em equipamentos rotativos
            
            ## Origens de dados
            - `Simulator` — dados gerados pelo SensorSimulator
            - `Interface` — dados inseridos manualmente pelo SensorInterface (WPF)
            """,
        Contact = new OpenApiContact
        {
            Name = "SIMI Dev Team",
            Email = "simi@industria.com"
        }
    });

    // Inclui os comentários XML gerados pelo compilador
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── CORS (permite acesso local durante desenvolvimento) ───────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Migração automática do banco na inicialização ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();
    db.Database.EnsureCreated();
}

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMI API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz: http://localhost:PORT/
});

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
