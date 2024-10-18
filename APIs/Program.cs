using Application;
using Application.Abstractions.Caching;
using Infrastructure;
using Infrastructure.Caching;
using Microsoft.OpenApi.Models;
using Persistence;
using Presentation;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.
builder.Services.Scan(selector => selector.FromAssemblies(Infrastructure.AssemblyReference.Assembly, Persistence.AssemblyReference.Assembly).AddClasses(false).AsImplementedInterfaces().WithScopedLifetime());

builder.Services.AddPersistence(configuration).AddApplication().AddPresentation().AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
       builder =>
       {
           _ = builder.WithOrigins("http://localhost:4200", "http://localhost:5173", "http://localhost:3000") 
                  .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
       });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Business Cards Catalogue APIs - OmranAlksour@gmail.com ",
        Version = "v1",
        Contact = new OpenApiContact()
        {
            Name = "Omran Alksour",
            Email = "omranalksour@gmail.com",
            Url = new Uri("https://www.linkedin.com/in/omran-alksour")
        },
    }); 
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();


var app = builder.Build();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

