
using ClinicBooking.Api.Data;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers(); // abilita i controller per gestire le richieste HTTP

builder.Services.AddControllers()
    // evita il loop infinito Appointment -> Doctor -> Appointments -> Doctor... in JSON
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddSwaggerGen(); // usa le informazioni raccolte da APIExplorer per costruire il documento OpenApi, che swagger UI userà per disegnare l'interfaccia
builder.Services.AddEndpointsApiExplorer(); // ispeziona i controller capire quali endpoint esistono

// PasswordService è stateless quindi va bene condividere una sola istanza per tutta l'app
builder.Services.AddSingleton<PasswordService>();


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
