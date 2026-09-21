
using ClinicBooking.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(); // abilita i controller per gestire le richieste HTTP

builder.Services.AddSwaggerGen(); // usa le informazioni raccolte da APIExplorer per costruire il documento OpenApi, che swagger UI userà per disegnare l'interfaccia
builder.Services.AddEndpointsApiExplorer(); // ispeziona i controller capire quali endpoint esistono

// registra AppDbContext nella Dependency Injection, collegato al file SQLite indicato in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // attivazione di Swagger a runtime
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
