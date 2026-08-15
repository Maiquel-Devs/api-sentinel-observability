using PaymentFlow.Diagnostics.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeta os diagnósticos passando a configuração do appsettings.json
builder.Services.AddSentinelDiagnostics(builder.Configuration);

var app = builder.Build();

// Ativa o middleware de captura global de exceções
app.UseSentinelDiagnostics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();