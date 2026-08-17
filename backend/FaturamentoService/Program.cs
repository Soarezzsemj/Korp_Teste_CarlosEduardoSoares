using FaturamentoService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Configuração da Documentação Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Korp ERP - Serviço de Faturamento";
        document.Info.Version = "v1.0";
        document.Info.Description =
            "Microsserviço de Faturamento desenvolvido para o projeto técnico de Sistema de Emissão de Notas Fiscais.\n\n" +
            "**Funcionalidades:**\n" +
            "- Emissão e fechamento de Notas Fiscais.\n" +
            "- Comunicação síncrona HTTP com o microsserviço de Estoque para validação, baixa física e estorno de saldos.\n" +
            "- Persistência física em banco de dados SQL Server.";

        return Task.CompletedTask;
    });
});

//comunicação com o EstoqueService
builder.Services.AddHttpClient("EstoqueService", client =>
{
    var baseUrl = builder.Configuration["EstoqueService:BaseUrl"] ?? "http://localhost:5190/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Execução automática de Migrations com retry para aguardar o SQL Server no Docker
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    for (int retry = 0; retry < 5; retry++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Banco de dados de Faturamento sincronizado e migrations aplicadas.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Aguardando SQL Server... Tentativa {retry + 1}/5. Detalhe: {ex.Message}");
            Thread.Sleep(3000);
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Teste Técnico Korp - Faturamento API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();