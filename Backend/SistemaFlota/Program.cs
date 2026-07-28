using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaFlota;
using SistemaFlota.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "https://flota.gecobagsci.com",
                "http://flota.gecobagsci.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(jwtKey!)
            )
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($">>> ENV: {builder.Environment.EnvironmentName}");
Console.WriteLine($">>> CONN: {connectionString}");

// ── MySQL con versión fija — evita AutoDetect en Railway ──────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(30);
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        }
    )
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditoriaService>();

// ── TWILIO ────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ITwilioService, FlotaChatService>();

builder.Services.AddScoped<IProveedorOrdenesProduccion, ImportacionExcelOrdenesService>();

builder.Services.AddHostedService<RecordatorioAutorizacionesService>();

// ── Zona horaria Colombia UTC-5 ───────────────────────────────────────────────
Environment.SetEnvironmentVariable("TZ", "America/Bogota");

// ── Puerto — solo Railway en producción ──────────────────────────────────────
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// =====================================
// SEED
// =====================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   db.Database.Migrate();


    // ── Semilla: Tipos de Formato de Calidad ──────────────────────────────
    if (!db.Set<TipoFormatoCalidad>().Any())
    {
        var extrusion = new TipoFormatoCalidad { Codigo = "F-GC-004", Nombre = "Extrusión", TieneVariablesCriticas = true };
        var impresion = new TipoFormatoCalidad { Codigo = "F-GC-005", Nombre = "Impresión", TieneVariablesCriticas = false };
        var sellado = new TipoFormatoCalidad { Codigo = "F-GC-006", Nombre = "Sellado", TieneVariablesCriticas = false };
        var precorte = new TipoFormatoCalidad { Codigo = "F-GC-007", Nombre = "Precorte", TieneVariablesCriticas = false };

        db.Set<TipoFormatoCalidad>().AddRange(extrusion, impresion, sellado, precorte);
        db.SaveChanges();

        var caracteristicas = new List<CaracteristicaFormato>
    {
        // Extrusión (F-GC-004)
        new() { TipoFormatoId = extrusion.Id, Orden = 1, Descripcion = "Medida de Película" },
        new() { TipoFormatoId = extrusion.Id, Orden = 2, Descripcion = "Calibre de Película" },
        new() { TipoFormatoId = extrusion.Id, Orden = 3, Descripcion = "Apariencia de Película" },
        new() { TipoFormatoId = extrusion.Id, Orden = 4, Descripcion = "Resistencia de Película" },
        new() { TipoFormatoId = extrusion.Id, Orden = 5, Descripcion = "Tratado Corona" },
        new() { TipoFormatoId = extrusion.Id, Orden = 6, Descripcion = "Bobinado del rollo" },
        new() { TipoFormatoId = extrusion.Id, Orden = 7, Descripcion = "Medida/Alineación Fuelles" },
        new() { TipoFormatoId = extrusion.Id, Orden = 8, Descripcion = "Grafilado" },
        new() { TipoFormatoId = extrusion.Id, Orden = 9, Descripcion = "Sellado en Película" },

        // Impresión (F-GC-005)
        new() { TipoFormatoId = impresion.Id, Orden = 1, Descripcion = "Medida de la Película" },
        new() { TipoFormatoId = impresion.Id, Orden = 2, Descripcion = "Calibre de la Película" },
        new() { TipoFormatoId = impresion.Id, Orden = 3, Descripcion = "Apariencia de la película" },
        new() { TipoFormatoId = impresion.Id, Orden = 4, Descripcion = "Tratado Corona" },
        new() { TipoFormatoId = impresion.Id, Orden = 5, Descripcion = "Referencia de Impresión" },
        new() { TipoFormatoId = impresion.Id, Orden = 6, Descripcion = "Registros de Impresión" },
        new() { TipoFormatoId = impresion.Id, Orden = 7, Descripcion = "Bobinado del Rollo" },

        // Sellado (F-GC-006)
        new() { TipoFormatoId = sellado.Id, Orden = 1, Descripcion = "Medida de la Bolsa" },
        new() { TipoFormatoId = sellado.Id, Orden = 2, Descripcion = "Calibre de la Bolsa" },
        new() { TipoFormatoId = sellado.Id, Orden = 3, Descripcion = "Apariencia de la película" },
        new() { TipoFormatoId = sellado.Id, Orden = 4, Descripcion = "Resistencia del Sellado" },
        new() { TipoFormatoId = sellado.Id, Orden = 5, Descripcion = "Línea de Sellado" },
        new() { TipoFormatoId = sellado.Id, Orden = 6, Descripcion = "Medida/Alineación fuelles" },
        new() { TipoFormatoId = sellado.Id, Orden = 7, Descripcion = "Perforaciones" },
        new() { TipoFormatoId = sellado.Id, Orden = 8, Descripcion = "Troquel/Manija" },

        // Precorte (F-GC-007)
        new() { TipoFormatoId = precorte.Id, Orden = 1, Descripcion = "Medida de la Bolsa" },
        new() { TipoFormatoId = precorte.Id, Orden = 2, Descripcion = "Calibre de la Bolsa" },
        new() { TipoFormatoId = precorte.Id, Orden = 3, Descripcion = "Resistencia del Sellado" },
        new() { TipoFormatoId = precorte.Id, Orden = 4, Descripcion = "Línea de Sellado" },
        new() { TipoFormatoId = precorte.Id, Orden = 5, Descripcion = "Línea de Precorte" },
        new() { TipoFormatoId = precorte.Id, Orden = 6, Descripcion = "Troquel" },
        new() { TipoFormatoId = precorte.Id, Orden = 7, Descripcion = "Tratado Corona" },
        new() { TipoFormatoId = precorte.Id, Orden = 8, Descripcion = "Bobinado del Rollo" },
    };

        db.Set<CaracteristicaFormato>().AddRange(caracteristicas);
        db.SaveChanges();
        Console.WriteLine("✅ Tipos de Formato de Calidad precargados");
    }

    try
    {

        var existeAdmin = await db.Usuarios.AnyAsync(u => u.Rol == "Admin" && u.Username == "admin");
        if (!existeAdmin)
        {
            db.Usuarios.Add(new Usuario
            {
                Username = "admin",
                Password = "admin123",
                Rol = "Admin",
                Email = "admin@sistemaflota.com",
                Activo = true
            });
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Usuario admin creado");
        }

        var existeMaestro = await db.Usuarios.AnyAsync(u => u.Username == "maestro_sf");
        if (!existeMaestro)
        {
            db.Usuarios.Add(new Usuario
            {
                Username = "maestro_sf",
                Password = "Fl0t@M4estr0#2024!",
                Rol = "Admin",
                Email = "maestro@sistemaflota.internal",
                Activo = true
            });
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Usuario maestro creado");
        }

        var existenTipos = await db.TiposVehiculo.AnyAsync();
        if (!existenTipos)
        {
            db.TiposVehiculo.AddRange(
                new TipoVehiculo { Nombre = "Camión" },
                new TipoVehiculo { Nombre = "Van" },
                new TipoVehiculo { Nombre = "Moto" },
                new TipoVehiculo { Nombre = "Motocarro" },
                new TipoVehiculo { Nombre = "Particular" },
                new TipoVehiculo { Nombre = "Furgón" },
                new TipoVehiculo { Nombre = "Tractocamión" },
                new TipoVehiculo { Nombre = "Otro" }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Tipos de vehículo creados.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en seed: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            Console.WriteLine($"❌ ERROR GLOBAL: {error.Error.Message}");
            Console.WriteLine($"❌ STACK: {error.Error.StackTrace}");
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(
                    new { error = error.Error.Message }
                )
            );
        }
    });
});

app.Run();