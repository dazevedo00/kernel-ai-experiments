using APIGenerativeAI.Cache;
using APIGenerativeAI.Controllers;
using APIGenerativeAI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

// Registrar o FindFunctionalityController para inje��o de depend�ncia
builder.Services.AddTransient<FindFunctionalityController>();
builder.Services.AddTransient<FunctionParameterController>();

// Adicionar o servi�o de cache em mem�ria
builder.Services.AddMemoryCache();

// Registrar o servi�o de estado da fun��o
builder.Services.AddSingleton<IFunctionStateService, FunctionStateService>();

// Configure the Semantic Kernel with OpenAI Chat Completion
builder.Services.AddSingleton(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: "gpt-3.5-turbo",
        apiKey: "INSERT_GIT_APIKEY"
    );
    return kernelBuilder.Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
