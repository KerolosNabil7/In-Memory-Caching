using In_Memory_Caching.Data;
using In_Memory_Caching.DTOs;
using In_Memory_Caching.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Register the in-memory caching service
builder.Services.AddMemoryCache();

//Register the SQLServer
var DefaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(DefaultConnectionString));

//Register Product service
builder.Services.AddTransient<IProductService, ProductService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Minimal API Endpoints
app.MapGet("/products", async (IProductService service) =>
{
    var products = await service.GetAll();
    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, IProductService service) =>
{
    var product = await service.Get(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (ProductCreationDto product, IProductService service) =>
{
    await service.Add(product);
    return Results.Created($"/products", product); // You can customize the location URI
});



app.Run();
