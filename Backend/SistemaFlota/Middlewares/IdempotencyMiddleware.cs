using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models.Idempotencia;

namespace SistemaFlota.Middlewares;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        AppDbContext db)
    {
        // Solo POST y PUT
        if (context.Request.Method != HttpMethods.Post &&
            context.Request.Method != HttpMethods.Put)
        {
            await _next(context);
            return;
        }

        // Si no viene Idempotency-Key,
        // dejamos pasar normalmente.
        if (!context.Request.Headers.TryGetValue(
                "Idempotency-Key",
                out var keyHeader))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = keyHeader.ToString().Trim();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        // Validar longitud máxima de la BD
        if (idempotencyKey.Length > 255)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "El Idempotency-Key no puede superar los 255 caracteres."
            });

            return;
        }

        // ============================================================
        // 1. BUSCAR SI YA EXISTE LA SOLICITUD
        // ============================================================

        var existente = await db.IdempotencyLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Key == idempotencyKey);

        if (existente != null)
        {
            // StatusCode = 0 significa que otra solicitud
            // todavía está procesando esta llave.
            if (existente.StatusCode == 0)
            {
                context.Response.StatusCode =
                    StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "La solicitud ya está siendo procesada."
                });

                return;
            }

            // ========================================================
            // Solicitud ya procesada → devolver respuesta guardada
            // ========================================================

            context.Response.StatusCode = existente.StatusCode;

            if (!string.IsNullOrWhiteSpace(existente.ContentType))
            {
                context.Response.ContentType =
                    existente.ContentType;
            }

            if (!string.IsNullOrWhiteSpace(existente.ResponseBody))
            {
                await context.Response.WriteAsync(
                    existente.ResponseBody,
                    Encoding.UTF8);
            }

            return;
        }

        // ============================================================
        // 2. RESERVAR LA LLAVE ANTES DE EJECUTAR EL ENDPOINT
        // ============================================================

        var usuarioId =
            context.User?.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        var log = new IdempotencyLog
        {
            Key = idempotencyKey,
            Method = context.Request.Method,
            Path = context.Request.Path,
            UsuarioId = usuarioId,

            // 0 = procesando
            StatusCode = 0,

            ContentType = null,
            ResponseBody = null,
            FechaCreacion = DateTime.UtcNow
        };

        try
        {
            db.IdempotencyLogs.Add(log);

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Otra solicitud ganó la carrera y creó
            // la misma llave primero.

            db.Entry(log).State = EntityState.Detached;

            var solicitudExistente =
                await db.IdempotencyLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Key == idempotencyKey);

            if (solicitudExistente != null &&
                solicitudExistente.StatusCode == 0)
            {
                context.Response.StatusCode =
                    StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "La solicitud ya está siendo procesada."
                });

                return;
            }

            if (solicitudExistente != null)
            {
                context.Response.StatusCode =
                    solicitudExistente.StatusCode;

                if (!string.IsNullOrWhiteSpace(
                        solicitudExistente.ContentType))
                {
                    context.Response.ContentType =
                        solicitudExistente.ContentType;
                }

                if (!string.IsNullOrWhiteSpace(
                        solicitudExistente.ResponseBody))
                {
                    await context.Response.WriteAsync(
                        solicitudExistente.ResponseBody,
                        Encoding.UTF8);
                }

                return;
            }

            throw;
        }

        // ============================================================
        // 3. CAPTURAR RESPUESTA DEL ENDPOINT
        // ============================================================

        var originalBody = context.Response.Body;

        await using var responseBody = new MemoryStream();

        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Leer respuesta
            responseBody.Position = 0;

            var responseText =
                await new StreamReader(responseBody)
                    .ReadToEndAsync();

            // ========================================================
            // 4. SI FUE EXITOSA → GUARDAR RESPUESTA
            // ========================================================

            if (context.Response.StatusCode >= 200 &&
                context.Response.StatusCode < 300)
            {
                log.StatusCode =
                    context.Response.StatusCode;

                log.ContentType =
                    context.Response.ContentType;

                log.ResponseBody =
                    responseText;

                await db.SaveChangesAsync();
            }
            else
            {
                // ====================================================
                // Si falló, eliminamos la reserva.
                // Así se puede volver a intentar con la misma llave.
                // ====================================================

                db.IdempotencyLogs.Remove(log);

                await db.SaveChangesAsync();
            }

            // ========================================================
            // 5. DEVOLVER RESPUESTA AL CLIENTE
            // ========================================================

            responseBody.Position = 0;

            await responseBody.CopyToAsync(originalBody);
        }
        catch
        {
            // ========================================================
            // Si el endpoint lanza una excepción,
            // liberamos la llave para permitir reintento.
            // ========================================================

            try
            {
                db.IdempotencyLogs.Remove(log);

                await db.SaveChangesAsync();
            }
            catch
            {
                // No ocultamos la excepción original.
            }

            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}