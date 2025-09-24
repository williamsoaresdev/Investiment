using CapitalGains.Application.Extensions;
using CapitalGains.Infrastructure.Extensions;
using CapitalGains.WebApi.Extensions;
using CapitalGains.WebApi.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Capital Gains API", 
        Version = "v1",
        Description = "API REST para cálculo de impostos sobre ganhos de capital em operações de compra e venda de ações",
        Contact = new() 
        {
            Name = "Capital Gains API",
            Email = "contact@capitalgains.com"
        }
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddWebApiServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Capital Gains API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Capital Gains API - Documentação";
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Capital Gains API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Capital Gains API - Documentação";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }