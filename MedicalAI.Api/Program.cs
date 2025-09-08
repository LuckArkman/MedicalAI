using MedicalAI.Api.Middleware;
using MedicalAI.Infrastructure.Interfaces;
using MedicalAI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configurar MediatR para buscar handlers no projeto de Aplicação
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(IGeminiService).Assembly));

// 3. Configurar as opções de settings do Gemini
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

// 4. Configurar HttpClient e o serviço Gemini
builder.Services.AddHttpClient<IGeminiService, GeminiService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();