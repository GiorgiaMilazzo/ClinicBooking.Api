# ClinicBooking.Api — Documentazione di ripasso del backend

Progetto di apprendimento ASP.NET Core Web API, costruito da zero tramite CLI (VS Code, non Visual Studio), ispirato alla struttura di un progetto analogo di un compagno di corso (TaskMaster) ma con dominio proprio: un sistema di **prenotazione visite mediche** (Doctor / Patient / Appointment).

Questo documento raccoglie la teoria incontrata passo passo, serve sia per ripassare sia per spiegare il progetto a chi lo legge per la prima volta.

---

## 1. Struttura del progetto

```
ClinicBooking.Api/
├── Controllers/          → endpoint HTTP (ricevono richieste, restituiscono risposte)
│   ├── DoctorsController.cs
│   ├── PatientsController.cs
│   ├── AppointmentsController.cs
│   ├── RegisterController.cs
│   └── LoginController.cs
├── Models/                → la "forma" dei dati, mappata da EF Core sulle tabelle
│   ├── Doctor.cs
│   ├── Patient.cs
│   ├── Appointment.cs     (contiene anche l'enum AppointmentStatus)
│   └── User.cs            (contiene anche l'enum UserRole)
├── Dtos/                  → i dati scambiati davvero con l'esterno (mai i Model grezzi)
│   ├── DoctorCreateDto.cs / DoctorDto.cs
│   ├── PatientCreateDto.cs / PatientDto.cs
│   ├── AppointmentCreateDto.cs / AppointmentUpdateDto.cs / AppointmentDto.cs
│   ├── RegisterDto.cs
│   └── LoginDto.cs
├── Data/
│   └── AppDbContext.cs    → il "ponte" fra le classi C# e il database
├── Services/
│   └── PasswordService.cs → hashing/verifica delle password
├── Migrations/             → cronologia delle modifiche allo schema del database (generate da EF Core)
├── Program.cs               → punto di ingresso e configurazione dell'app
├── appsettings.json         → configurazione (connection string, Jwt:Issuer/Audience — MAI segreti)
└── clinicbooking.db         → database SQLite (NON versionato su git)
```

**Perché questa separazione (Models vs Dtos vs Controllers vs Services)?**
Ogni cartella ha una responsabilità unica: i Model descrivono solo la forma dei dati per il database; i DTO descrivono solo cosa entra/esce davvero dall'API; i Controller gestiscono solo le richieste HTTP; i Service contengono logica riusabile (es. hashing) scollegata da HTTP e database.

---

## 2. Setup iniziale (senza Visual Studio)

Il progetto è stato creato da terminale, non con la procedura guidata grafica:
```
dotnet new webapi -o ClinicBooking.Api --use-controllers
```
- `dotnet new` → genera file a partire da un template.
- `--use-controllers` → sceglie lo stile con **Controller classici** (una classe per gruppo di endpoint) invece delle **Minimal API** (funzioni singole in `Program.cs`), lo stile di default dal .NET 6 in poi.

### Mac vs Windows
Il progetto è stato sviluppato prima su Mac poi su Windows. Due differenze gestite:
- **Database**: SQLite (file singolo, multipiattaforma) invece di SQL Server LocalDB (solo Windows) — scelta fatta apposta per portabilità.
- **dotnet ef**: se non disponibile, va installato come tool globale con `dotnet tool install --global dotnet-ef`.

---

## 3. `Program.cs` — le due fasi

`Program.cs` si esegue **dall'alto verso il basso** come uno script (diverso da una classe normale). La riga `var app = builder.Build();` segna un confine netto fra due fasi concettualmente diverse:

- **Fase 1 (prima di `Build()`)** — "cosa serve alla mia app per funzionare?": si registrano i **servizi** nella Dependency Injection (`AddControllers`, `AddDbContext`, `AddSwaggerGen`, `AddAuthentication`, `AddCors`, `AddSingleton<PasswordService>`...). Nessuna richiesta HTTP viene ancora gestita qui.
- **Fase 2 (dopo `Build()`)** — "come gestisco una richiesta che arriva?": si definisce la **pipeline**, cioè la sequenza di passaggi che ogni richiesta attraversa (`UseHttpsRedirection`, `UseCors`, `UseAuthentication`, `UseAuthorization`, `MapControllers`).

**Dependency Injection (DI)**: invece che ogni classe costruisca da sé le proprie dipendenze, le si registra una volta in Fase 1 e il framework le "inietta" automaticamente nel costruttore di chi le richiede (es. ogni Controller riceve `AppDbContext` nel costruttore). Vantaggio: se cambia come si costruisce una dipendenza, si cambia in un punto solo.

**Lifetime dei servizi** (quanto dura un'istanza):
- `AddSingleton` → una sola istanza condivisa per tutta la vita dell'app. Usato per `PasswordService`: è **stateless** (nessun dato interno che "ricorda" fra una chiamata e l'altra), quindi è sicuro condividerla.
- `AddDbContext` (di default `Scoped`) → una nuova istanza per ogni richiesta HTTP. Necessario perché il `DbContext` fa **change tracking** (tiene traccia di cosa sta caricando/modificando): condividere la stessa istanza fra richieste parallele causerebbe dati mischiati fra utenti diversi.

**Ordine importante nella pipeline (Fase 2)**: `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers`. Prima il server deve sapere *chi sei* (autenticazione), solo dopo può decidere *cosa ti è permesso fare* (autorizzazione).

---

## 4. I Model e le relazioni

Un Model (classe in `Models/`) rappresenta **solo la forma dei dati**, nessuna logica di accesso al database o di business — è un **POCO** (Plain Old CLR Object).

### Relazioni fra Doctor / Patient / Appointment
- `Doctor` → molti `Appointment` (1-a-molti)
- `Patient` → molti `Appointment` (1-a-molti)
- `Appointment` è il "ponte" fra i due, con due Foreign Key separate (`DoctorId`, `PatientId`) — non serve una relazione molti-a-molti diretta fra Doctor e Patient.

### Foreign Key e Navigation Property
```csharp
public int DoctorId { get; set; }        // FK — colonna reale nel database, sempre valorizzata
public Doctor? Doctor { get; set; }       // Navigation Property — NON è una colonna reale
```
Una **Navigation Property** permette di "spostarsi" da un'entità a quella collegata scrivendo `appointment.Doctor` invece di query manuali — ma **non esiste come colonna nel database**. È **nullable** (`Doctor?`) anche se la FK è obbligatoria perché EF Core non la carica automaticamente: resta `null` finché non la richiedi esplicitamente con `Include(...)`.

### Proprietà vs Campo
```csharp
public string Name { get; set; }   // proprietà — EF Core la vede e la mappa
public string Name;                 // campo — EF Core di norma la ignora
```
Su un Model destinato a EF Core, usare sempre proprietà (`{ get; set; }`), mai campi nudi.

### `required` e nullable reference types
Con `<Nullable>enable</Nullable>` nel `.csproj`, il compilatore distingue `string` (mai null) da `string?` (può essere null). Un campo `string` senza valore iniziale dà un warning (CS8618): si risolve con un default (`= string.Empty`, scelta più "morbida") oppure con `required` (il chiamante **deve** valorizzarlo, altrimenti errore di compilazione o, su un DTO ricevuto da HTTP, errore di validazione 400 automatico — scelta più rigorosa). Nel progetto: i Model usano `= string.Empty`, i DTO usano `required` sui campi davvero obbligatori.

### Enum
```csharp
public enum AppointmentStatus { booked, confirmed, complete }
public enum UserRole { admin, doctor }
```
Un `enum` obbliga un valore a essere uno fra pochi validi (invece di una stringa libera soggetta a errori). Viene salvato nel database come **intero** (0, 1, 2...) in base all'ordine di dichiarazione — attenzione a non riordinare i valori dopo aver già salvato dati veri, altrimenti cambia silenziosamente il significato dei numeri esistenti.

---

## 5. EF Core, DbContext e Migrations

**`AppDbContext`** è la classe che rappresenta la connessione al database e traduce le classi C# in tabelle:
```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<User> Users { get; set; }
}
```
- `DbSet<T>` = una tabella, ed è anche il punto di partenza per le query (`_context.Doctors.Where(...)`).
- `DbContextOptions<AppDbContext>` è **generico**: il `<AppDbContext>` etichetta quelle opzioni come specifiche di *questo* DbContext (utile se un domani ci fossero più DbContext nello stesso progetto).
- Sintassi `AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)` → *primary constructor* (C# 12): scorciatoia che passa il parametro direttamente al costruttore della classe base.

**Migrations**: generare e applicare sono due passi distinti.
```
dotnet ef migrations add NomeMigration     # genera solo il file (nessun effetto reale sul DB)
dotnet ef database update                  # applica davvero le modifiche al database
```
Separarli permette di **rivedere** cosa sta per succedere prima di eseguirlo per davvero (importante su un database già popolato di dati reali).

**Provider**: `Microsoft.EntityFrameworkCore.Sqlite` (database = un singolo file, zero configurazione) al posto di `SqlServer` usato dal compagno, per compatibilità multipiattaforma. Connection string:
```json
"DefaultConnection": "Data Source=clinicbooking.db"
```

---

## 6. Controller e CRUD

Un Controller riceve una richiesta HTTP, la traduce in un'operazione su `AppDbContext` (ricevuto via DI), restituisce una risposta.

```csharp
[ApiController]                    // validazione automatica input + risposte JSON di default
[Route("api/[controller]")]        // [controller] → nome della classe senza "Controller"
public class DoctorsController : ControllerBase
```

**`async`/`await`**: le operazioni su database sono I/O (parlano con qualcosa di esterno, il file `.db`), possono richiedere tempo. `async`/`await` permette al server di gestire altre richieste nel frattempo invece di restare bloccato. Regola pratica: ogni metodo EF Core che finisce in `Async` va atteso con `await`.

### Metodi di query usati e quando
| Metodo | Uso |
|---|---|
| `FindAsync(id)` | cerca per **chiave primaria**, non si può combinare con `Include` |
| `FirstOrDefaultAsync(x => condizione)` | cerca con una condizione esplicita, **si può** combinare con `Include` |
| `ToListAsync()` | restituisce tutta la lista |
| `AnyAsync(x => condizione)` | restituisce solo `true`/`false` — usato per controlli di esistenza (es. doppia prenotazione, email già registrata) |
| `Include(x => x.Relazione)` | carica anche una Navigation Property, altrimenti resta `null` |

### Status code HTTP usati
- `200 OK` — richiesta riuscita, con dati
- `201 Created` — risorsa creata (`CreatedAtAction`, indica dove trovarla)
- `204 No Content` — riuscita, niente da restituire (PUT, DELETE)
- `400 Bad Request` — dati inviati non validi (es. FK inesistente, vincolo di business violato)
- `401 Unauthorized` — nessun token valido, il server non sa chi sei
- `403 Forbidden` — token valido, ma ruolo insufficiente
- `404 Not Found` — risorsa cercata per Id non esiste

### Vincolo di business reale
In `AppointmentsController`, prima di creare un appuntamento si verifica: che `Doctor`/`Patient` esistano davvero (altrimenti `400`), e che quel medico non abbia già un altro appuntamento nello stesso orario (`AnyAsync`, altrimenti `400`).

---

## 7. DTO (Data Transfer Object)

**Perché**: esporre direttamente i Model crea due problemi concreti incontrati nel progetto:
1. **In input**: il chiamante potrebbe mandare un `Id` o una lista `Appointments` indesiderati nel body.
2. **In output**: le Navigation Property bidirezionali causano un **ciclo infinito di serializzazione JSON** (`Appointment → Doctor → Appointments → Doctor → ...`), errore 500.

Un DTO contiene *solo* i dati che ha senso scambiare in quel momento specifico — con classi **separate** per scopi diversi:
- `XCreateDto` (input creazione) — solo i campi che il chiamante può legittimamente scegliere (mai `Id`, mai dati decisi dal server come `RegistrationDate` o lo `Status` iniziale di un Appointment).
- `XUpdateDto` (input modifica) — può differire dal Create: es. `AppointmentUpdateDto` non include `DoctorId`/`PatientId` (riassegnare un appuntamento a un altro medico non è un "aggiornamento", semmai si cancella e se ne crea uno nuovo).
- `XDto` (output) — niente Navigation Property; può fare **flattening**: "appiattire" un dato annidato (`appointment.Doctor.Name`) in un campo semplice del DTO (`DoctorName`), evitando di esporre l'intero oggetto collegato.

La conversione fra Model e DTO è **manuale**, in entrambe le direzioni, dentro ogni metodo del controller (nessuna libreria di mapping automatico usata qui).

*(Soluzione alternativa, scartata dopo averla provata: `options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles` in `Program.cs` — "spegne" il ciclo invece di evitarlo alla radice. I DTO sono la soluzione corretta e definitiva.)*

---

## 8. Autenticazione e autorizzazione (JWT)

**Due concetti distinti**: *autenticazione* = "chi sei?" (login); *autorizzazione* = "cosa ti è permesso fare, dato chi sei?" (ruoli).

### Flusso
1. **Registrazione** (`RegisterController`) — la password viene **hashata**, mai salvata in chiaro.
2. **Login** (`LoginController`) — verifica le credenziali, genera un **token JWT**.
3. **Richieste successive** — il client allega il token (header `Authorization: Bearer ...`); il server lo legge e sa chi sta chiamando, senza richiedere di nuovo le credenziali (server "stateless").

### Struttura di un JWT
`header.payload.signature` — il **payload** contiene le *Claims* (dati sull'utente: Id, Nome, Ruolo), **codificate ma non cifrate** (mai dati sensibili qui). La **signature** è una firma crittografica fatta con una chiave segreta nota solo al server, garantisce che il token non sia stato manomesso.

> ⚠️ **Punto importante imparato sul campo**: il ruolo dentro il token è una "fotografia" presa al momento del login — se il ruolo dell'utente cambia nel database *dopo*, un token già emesso continua a riportare il ruolo vecchio finché non scade o non si rifà login.

### `PasswordService`
```csharp
private readonly PasswordHasher<User> _hasher = new();
public string HashPassword(User user, string plainPassword) => _hasher.HashPassword(user, plainPassword);
public bool VerifyPassword(User user, string hashedPassword, string plainPassword)
    => _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword) == PasswordVerificationResult.Success;
```
Non si scrive mai un proprio algoritmo di hashing: si usa `Microsoft.AspNetCore.Identity.PasswordHasher<T>`, già disponibile nel framework condiviso ASP.NET Core (non richiede un pacchetto NuGet esplicito).

### Generazione del token (`LoginController`)
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.UserName),
    new Claim(ClaimTypes.Role, user.Role.ToString())
};
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: credentials);
```

### Validazione del token (`Program.cs`, Fase 1)
```csharp
builder.Services.AddAuthentication(options => { /* schema di default = JwtBearer */ })
    .AddJwtBearer(options => { options.TokenValidationParameters = new TokenValidationParameters { ... }; });
```
E Fase 2: `app.UseAuthentication();` **prima di** `app.UseAuthorization();`.

### Protezione degli endpoint
```csharp
[Authorize(Roles = "admin")]   // sopra l'intera classe controller
```
Richiede non solo un token valido, ma che la Claim `Role` corrisponda esattamente — altrimenti `403`, non `401` (il server sa chi sei, ma non ti basta).

### Swagger + JWT
Per testare endpoint protetti da Swagger serve configurare esplicitamente `AddSecurityDefinition`/`AddSecurityRequirement` in `AddSwaggerGen` (altrimenti non compare il pulsante "Authorize"). **Nota di versione**: con Swashbuckle.AspNetCore 10.x, i tipi sono in `Microsoft.OpenApi` (non più `Microsoft.OpenApi.Models`), e la sintassi di `AddSecurityRequirement` richiede una funzione `document => ...` con `OpenApiSecuritySchemeReference` — cambiata rispetto a versioni precedenti della libreria.

---

## 9. CORS

Il browser blocca di default le richieste da un'origine (dominio/porta) verso un'altra — problema che riguarda **solo** i client eseguiti nel browser (es. Blazor WebAssembly), non Swagger (stessa origine dell'API) né app native/Postman.

```csharp
// Fase 1
builder.Services.AddCors(options => options.AddPolicy("BlazorPolicy", policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Fase 2 — dopo UseHttpsRedirection, prima di UseAuthentication
app.UseCors("BlazorPolicy");
```
`AllowAnyOrigin()` va bene in sviluppo; in un deploy reale andrebbe ristretto a un dominio preciso con `WithOrigins(...)` (lasciarlo aperto in produzione è un rischio di sicurezza).

---

## 10. Gestione dei segreti — User Secrets

La chiave JWT **non va mai** scritta/committata in `appsettings.json` (file versionato su GitHub). Si usa invece lo strumento **User Secrets**, che salva il valore in un file fuori dal repository, solo sulla macchina locale:
```
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "..."
```
Nessuna modifica al codice C#: `builder.Configuration["Jwt:Key"]` legge in automatico sia da `appsettings.json` sia dai User Secrets, in modo trasparente.

---

## 11. Pacchetti NuGet installati

| Pacchetto | A cosa serve |
|---|---|
| `Microsoft.AspNetCore.OpenApi` | generato di default dal template, descrizione OpenAPI "nuda" (senza interfaccia grafica) |
| `Swashbuckle.AspNetCore` | genera la Swagger UI, l'interfaccia web per esplorare/testare gli endpoint |
| `Microsoft.EntityFrameworkCore.Sqlite` | provider EF Core per SQLite (ORM + driver database) |
| `Microsoft.EntityFrameworkCore.Tools` | abilita i comandi `dotnet ef migrations`/`database update` da terminale |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | classi per generare/validare i token JWT (`JwtSecurityToken`, `TokenValidationParameters`, `AddJwtBearer`...) |

*(`Microsoft.AspNetCore.Identity`, che fornisce `PasswordHasher<T>`, **non** richiede un riferimento NuGet esplicito: fa già parte del framework condiviso ASP.NET Core — installarlo dà il warning NU1510 ed è stato rimosso dal `.csproj`.)*

### Tool globali
```
dotnet tool install --global dotnet-ef   # se dotnet ef non è già disponibile sulla macchina
```

---

## 12. Comandi da terminale usati (riepilogo cronologico)

```bash
# Creazione progetto
dotnet new webapi -o ClinicBooking.Api --use-controllers
dotnet new gitignore

# Pacchetti
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Database
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations add AddUserTable
dotnet ef database update

# Segreti
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "..."

# Build e run
dotnet build
dotnet watch run

# Git
git init / git clone
git add . / git commit -m "..." / git push
git rm -r --cached bin obj          # rimuove dal tracking senza cancellare dal disco
git rm --cached clinicbooking.db
```

---

## 13. Errori incontrati e lezioni pratiche (vale la pena ricordarle)

- **File non salvato** → "il tipo/namespace non esiste" anche con codice corretto: controllare sempre il pallino sulla tab di VS Code prima di dare la colpa al codice.
- **`.gitignore` mancante o creato dopo il primo `git add .`** → `bin/`, `obj/`, il `.db` finiscono tracciati comunque; il `.gitignore` previene, non è retroattivo. Si risolve con `git rm -r --cached`.
- **Modifiche alla Fase 1 di `Program.cs`** (registrazione servizi) spesso **non si applicano con l'hot reload** di `dotnet watch` — serve un riavvio vero (`Ctrl+R` o `Ctrl+C` + rilancio).
- **Un errore 500 non significa che nulla sia stato salvato**: `SaveChangesAsync()` può riuscire anche se un passo *successivo* (es. serializzare la risposta) fallisce.
- **Versioni delle librerie cambiano le API**: la sintassi di `AddSecurityRequirement` per Swagger+JWT è cambiata fra versioni di Swashbuckle — in caso di dubbio, controllare la documentazione della versione installata, non fidarsi di esempi trovati genericamente.

---
