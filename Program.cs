using ClinicBooking.Api.Data;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers(); // abilita i controller per gestire le richieste HTTP

builder.Services.AddControllers()
    // evita il loop infinito Appointment -> Doctor -> Appointments -> Doctor... in JSON
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

//builder.Services.AddSwaggerGen(); // usa le informazioni raccolte da APIExplorer per costruire il documento OpenApi, che swagger UI userà per disegnare l'interfaccia

builder.Services.AddSwaggerGen(options =>
{
    // dice a Swagger che esiste un modo per autenticarsi (Bearer JWT) -> fa comparire il pulsante "Authorize"
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Inserisci il token JWT (senza scrivere 'Bearer ', ci pensa Swagger)"
    });

    // applica quel sistema di sicurezza a tutti gli endpoint -> fa comparire il lucchetto e allega il token alle richieste di test
    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddEndpointsApiExplorer(); // ispeziona i controller capire quali endpoint esistono

// PasswordService è stateless quindi va bene condividere una sola istanza per tutta l'app
builder.Services.AddSingleton<PasswordService>();

// registra AppDbContext nella Dependency Injection, collegato al file SQLite indicato in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// registrazione del sistema di autenticazione JWT
builder.Services.AddAuthentication(options =>

{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
}
);

// CORS meccanismo che dice esplicitamente al server di accettare richieste anche da un'altra origine
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.AllowAnyOrigin() // la reale porta di blazor
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // attivazione di Swagger a runtime
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS prima del controllo di autenticazione
app.UseCors("BlazorPolicy");

// l'ordine è importante
// prima chi sei, poi cosa puoi fare

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
