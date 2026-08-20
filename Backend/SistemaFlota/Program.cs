using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaFlota;
using QuestPDF.Infrastructure;
using SistemaFlota.Configuracion;
using SistemaFlota.Models;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;
using SistemaFlota.Services.Costos.Inventario;
using SistemaFlota.Services.Costos.Inventario.CortesInventario;
using SistemaFlota.Services.Costos.Materiales;
using SistemaFlota.Services.Costos.OrdenCompra;
using SistemaFlota.Services.Costos.OrdenesTraslado;
using SistemaFlota.Services.Costos.Proveedores;
using SistemaFlota.Services.Costos.RecepcionMercancia;
using SistemaFlota.Services.Email;
using SistemaFlota.Services.ImpresionEtiquetas;
using SistemaFlota.Services.Notificaciones;
using SistemaFlota.Services.Pdf.RecepcionMercancia;
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

// CORS 
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
Console.WriteLine($">>> Ambiente: {builder.Environment.EnvironmentName}");
Console.WriteLine($">>> Tiene conexion: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

// -- MySQL con versión fija — evita AutoDetect en Railway ----------------------
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

// Servicios
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMaterialesService, MaterialService>();
builder.Services.AddScoped<IOrdenCompraService, OrdenCompraService>();
builder.Services.AddScoped<IOrdenCompraPdfService, OrdenCompraPdfService>();
builder.Services.AddScoped<IConsecutivoService, ConsecutivoService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRecepcionMercanciaService, RecepcionMercanciaService>();
builder.Services.AddScoped<IEtiquetasPdfService, EtiquetasPdfService>();
builder.Services.AddScoped<IRecepcionMercanciaPdfService, RecepcionMercanciaPdfService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IAjusteInventarioService, AjusteInventarioService>();
builder.Services.AddScoped<ICorteInventarioService, CorteInventarioService>();
builder.Services.AddScoped<IOrdenTrasladoService, OrdenTrasladoService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.AddScoped<INotificacionRecepcionService, NotificacionRecepcionService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditoriaService>();

// -- TWILIO --------------------------------------------------------------------
builder.Services.AddSingleton<IMensajeriaService, FlotaChatService>();

builder.Services.AddScoped<IProveedorOrdenesProduccion, ImportacionExcelOrdenesService>();

builder.Services.AddHostedService<RecordatorioAutorizacionesService>();

// -- Zona horaria Colombia UTC-5 -----------------------------------------------
Environment.SetEnvironmentVariable("TZ", "America/Bogota");

//  Puerto â€” solo Railway en produccion 
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

QuestPDF.Settings.License = LicenseType.Community;
var app = builder.Build();

// SEED
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   db.Database.Migrate();


    // -- Semilla: Tipos de Formato de Calidad ------------------------------
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
        Console.WriteLine("? Tipos de Formato de Calidad precargados");
    }

    // -- Semilla: Opciones de Formulario (Máquina, Corona, Molde, Operario) --
    if (!db.Set<OpcionFormulario>().Any())
    {
            var opciones = new List<OpcionFormulario>();

            // Operarios (aplica a TODOS los formatos: TipoFormatoId = null)
            var operarios = new[] { "Mauricio Figueroa", "Dario Ossa", "Asmed Cano", "Jhon Jairo Montes" };
            for (int i = 0; i < operarios.Length; i++)
                opciones.Add(new OpcionFormulario { Categoria = "Operario", TipoFormatoId = null, Valor = operarios[i], Orden = i });

            // Máquina (por ahora, aplica a todos — se puede diferenciar por formato después desde la pantalla de admin)
            var maquinas = new[] { "Coextrusora 50", "Coextrusora 40", "Extrusora 60", "50.3", "50.1", "47.4", "47.3" };
            for (int i = 0; i < maquinas.Length; i++)
                opciones.Add(new OpcionFormulario { Categoria = "Maquina", TipoFormatoId = null, Valor = maquinas[i], Orden = i });

            // Corona (aplica a todos)
            var coronas = new[] { "Alta", "Baja" };
            for (int i = 0; i < coronas.Length; i++)
                opciones.Add(new OpcionFormulario { Categoria = "Corona", TipoFormatoId = null, Valor = coronas[i], Orden = i });

            // Molde (aplica a todos)
            var moldes = new[] { "75 mm", "55 mm", "40 mm", "45 mm", "60 mm", "80 mm", "90 mm", "5.5 pulg", "4 pulg", "2 pulg", "8 pulg", "6 pulg", "11.5 pulg", "7 pulg", "5 pulg" };
            for (int i = 0; i < moldes.Length; i++)
                opciones.Add(new OpcionFormulario { Categoria = "Molde", TipoFormatoId = null, Valor = moldes[i], Orden = i });

            db.Set<OpcionFormulario>().AddRange(opciones);
            db.SaveChanges();
            Console.WriteLine("? Opciones de Formulario precargadas");
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

            Console.WriteLine(" Usuario admin creado");
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

            Console.WriteLine(" Usuario maestro creado");
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

            Console.WriteLine("? Tipos de vehículo creados.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error en seed: {ex.Message}");
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
            Console.WriteLine($"? ERROR GLOBAL: {error.Error.Message}");
            Console.WriteLine($"? STACK: {error.Error.StackTrace}");

            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(
                    new { error = error.Error.Message }
                )
            );
        }
    });
});

app.Run();
