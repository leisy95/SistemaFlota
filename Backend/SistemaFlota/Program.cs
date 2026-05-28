using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SistemaFlota;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// CONTROLLERS
// =====================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// =====================================
// SWAGGER
// =====================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================
// CORS ANGULAR
// =====================================
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AngularPolicy",
        policy =>
        {
            policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
        });
});

// =====================================
// JWT
// =====================================
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

// =====================================
// MYSQL
// =====================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
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

// =====================================
// AUDITORÍA
// =====================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditoriaService>();

// =====================================
// APP
// =====================================
var app = builder.Build();

// =====================================
// SWAGGER
// =====================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================
// CORS HEADERS PARA IMÁGENES ESTÁTICAS
// =====================================
app.Use(async (context, next) =>
{
    context.Response.Headers.Append(
        "Access-Control-Allow-Origin", "*"
    );
    await next();
});

// =====================================
// ARCHIVOS ESTÁTICOS
// =====================================
app.UseStaticFiles();

// =====================================
// CORS
// =====================================
app.UseCors("AngularPolicy");

// =====================================
// AUTH
// =====================================
app.UseAuthentication();
app.UseAuthorization();

// =====================================
// MAP
// =====================================
app.MapControllers();

// =====================================
// RUN
// =====================================

// =====================================
// MANEJO DE ERRORES GLOBAL
// =====================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            Console.WriteLine($"❌ ERROR GLOBAL: {error.Error.Message}");
            Console.WriteLine($"❌ STACK: {error.Error.StackTrace}");
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = error.Error.Message
                })
            );
        }
    });
});
app.Run();