using FaturamentoService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Korp ERP - Serviço de Faturamento";
        document.Info.Version = "v1.0";
        document.Info.Description =
            "Microsserviço de Faturamento desenvolvido por Carlos Eduardo Soares Souza Santos para o Projeto técnico de Sistema de emissão de Notas Fiscais.\n\n" +
            "**Mais sobre mim:**\n" +
            "- **Portfólio:** [soarezzsemj.github.io/Portfolio-Carlos-Eduardo](https://soarezzsemj.github.io/Portfolio-Carlos-Eduardo/)\n" +
            "- **GitHub:** [github.com/Soarezzsemj](https://github.com/Soarezzsemj)\n" +
            "- **LinkedIn:** [linkedin.com/in/carlos-eduardo-soares](https://www.linkedin.com/in/carlos-eduardo-soares-081419343/)\n\n" +
            "**Funcionalidades:**\n" +
            "- Emissão de Notas Fiscais sequenciais com status inicial 'Aberta'.\n" +
            "- Impressão/Fechamento de notas integrado ao microsserviço de Estoque.\n" +
            "- Tratamento de resiliência caso o serviço de estoque esteja indisponível.\n" +
            "- Cancelamento de notas com estorno automático de saldo.";

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

builder.Services.AddHttpClient("EstoqueService", client =>
{
    var estoqueUrl = builder.Configuration.GetValue<string>("EstoqueService:BaseUrl") ?? "http://localhost:5190";
    client.BaseAddress = new Uri(estoqueUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Korp ERP - Faturamento API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();