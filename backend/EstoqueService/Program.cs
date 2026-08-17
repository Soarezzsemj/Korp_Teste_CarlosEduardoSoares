using EstoqueService.Data;
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

// Configuração do Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Korp ERP - Serviço de Estoque";
        document.Info.Version = "v1.0";
        document.Info.Description =
            "Microsserviço de Estoque desenvolvido por Carlos Eduardo Soares Souza Santos para o Projeto técnico de Sistema de emissão de Notas Fiscais.\n\n" +
            "**Mais sobre mim:**\n" +
            "- **Portfólio:** [soarezzsemj.github.io/Portfolio-Carlos-Eduardo](https://soarezzsemj.github.io/Portfolio-Carlos-Eduardo/)\n" +
            "- **GitHub:** [github.com/Soarezzsemj](https://github.com/Soarezzsemj)\n" +
            "- **LinkedIn:** [linkedin.com/in/carlos-eduardo-soares](https://www.linkedin.com/in/carlos-eduardo-soares-081419343/)\n\n" +
            "**Funcionalidades:**\n" +
            "- Cadastro, edição e consulta de produtos e saldos.\n" +
            "- Abatimento e devolução de estoque com concorrência otimista (RowVersion).\n" +
            "- Persistência física em banco de dados SQL Server.";

        document.Info.Contact = new()
        {
            Name = "Carlos Eduardo Soares Souza Santos",
            Url = new Uri("https://soarezzsemj.github.io/Portfolio-Carlos-Eduardo/")
        };

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Execução automática de Migrations com retry para Docker
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
            logger.LogInformation("Banco de dados sincronizado e migrations aplicadas.");
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
            .WithTitle("Teste Técnico Korp - Estoque API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();