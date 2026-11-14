
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.Clientes;
using SistemaFlota.Models.InventariosBodega.Clientes;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.InventarioBodega.Clientes
{
    public class ClienteBodegaService : BaseService, IClienteBodegaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ClienteBodegaService(
           AppDbContext context,
           ICurrentUserService currentUser,
           SistemaFlota.AuditoriaService auditoria)
           : base(auditoria, currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // LISTAR CLIENTES CON FILTROS Y PAGINACIÓN
        public async Task<ClientesBodegaPaginadosDto> ListarClientesAsync(
            ClienteBodegaFiltroDto filtros)
        {
            int pagina = Math.Max(1, filtros.Pagina);
            int pageSize = Math.Clamp(filtros.PageSize, 1, 100);

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking();

            if (filtros.Activo.HasValue)
            {
                consulta = consulta.Where(c => c.Activo == filtros.Activo.Value);
            }

            // BÚSQUEDA GENERAL
            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                string buscar = filtros.Buscar.Trim();

                consulta = consulta.Where(c =>
                    c.Nombre.Contains(buscar) ||
                    (c.Direccion != null && c.Direccion.Contains(buscar)) ||
                    (c.Telefono != null && c.Telefono.Contains(buscar)) ||
                    (c.Email != null && c.Email.Contains(buscar))
                );
            }

            // FILTRO POR NOMBRE
            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                string nombre = filtros.Nombre.Trim();

                consulta = consulta.Where(c =>
                    c.Nombre.Contains(nombre));
            }

            // FILTRO POR TELÉFONO
            if (!string.IsNullOrWhiteSpace(filtros.Telefono))
            {
                string telefono = filtros.Telefono.Trim();

                consulta = consulta.Where(c =>
                    c.Telefono != null &&
                    c.Telefono.Contains(telefono));
            }

            // FILTRO POR EMAIL
            if (!string.IsNullOrWhiteSpace(filtros.Email))
            {
                string email = filtros.Email.Trim();

                consulta = consulta.Where(c =>
                    c.Email != null &&
                    c.Email.Contains(email));
            }

            // TOTAL DE REGISTROS FILTRADOS
            int total = await consulta.CountAsync();

            // TOTAL DE PÁGINAS
            int totalPaginas = (int)Math.Ceiling(
                total / (double)pageSize);

            // AJUSTAR PÁGINA SI SUPERA EL TOTAL
            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            // OBTENER CLIENTES PAGINADOS
            var items = await consulta
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Id)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = c.Activo
                })
                .ToListAsync();

            return new ClientesBodegaPaginadosDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                PageSize = pageSize,
                TotalPaginas = totalPaginas
            };
        }

        // OBTENER CLIENTE POR ID
        public async Task<ClienteResponseDto?> ObtenerClienteAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Email = c.Email
                })
                .FirstOrDefaultAsync();
        }

        // CREAR CLIENTE
        public async Task<RespuestaClienteDto> CrearClienteAsync(
            ClienteRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El nombre del cliente es obligatorio."
                };
            }

            var cliente = new Cliente
            {
                Nombre = dto.Nombre.Trim(),
                Direccion = dto.Direccion?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Email = dto.Email?.Trim(),
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            await AuditarAsync(
                "Crear",
                "ClientesBodega",
                $"Se creó el cliente {cliente.Nombre}.",
                cliente.Id);

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente creado correctamente.",
                Id = cliente.Id
            };
        }

        // ACTUALIZAR CLIENTE
        public async Task<RespuestaClienteDto> ActualizarClienteAsync(
            int id,
            EditarClienteRequestDto dto)
        {
            if (id <= 0 || dto.Id != id)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El identificador del cliente no es válido."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El nombre del cliente es obligatorio."
                };
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "Cliente no encontrado."
                };
            }

            cliente.Nombre = dto.Nombre.Trim();
            cliente.Direccion = dto.Direccion?.Trim();
            cliente.Telefono = dto.Telefono?.Trim();
            cliente.Email = dto.Email?.Trim();
            cliente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await AuditarAsync(
                "Actualizar",
                "ClientesBodega",
                $"Se actualizó el cliente {cliente.Nombre}.",
                cliente.Id);

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente actualizado correctamente.",
                Id = cliente.Id
            };
        }

        // DESACTIVAR CLIENTE
        public async Task<RespuestaClienteDto> DesactivarClienteAsync(int id)
        {
            if (id <= 0)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El identificador del cliente no es válido."
                };
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "Cliente no encontrado."
                };
            }

            if (!cliente.Activo)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El cliente ya se encuentra desactivado."
                };
            }

            // DESACTIVAR CLIENTE
            cliente.Activo = false;
            cliente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            // AUDITORÍA
            await AuditarAsync(
                "Desactivar",
                "ClientesBodega",
                $"Se desactivó el cliente {cliente.Nombre}.",
                cliente.Id);

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente desactivado correctamente.",
                Id = id
            };
        }

        // REACTIVAR CLIENTE
        public async Task<RespuestaClienteDto> ReactivarClienteAsync(int id)
        {
            if (id <= 0)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El identificador del cliente no es válido."
                };
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "Cliente no encontrado."
                };
            }

            if (cliente.Activo)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "El cliente ya se encuentra activo."
                };
            }

            // REACTIVAR CLIENTE
            cliente.Activo = true;
            cliente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            // AUDITORÍA
            await AuditarAsync(
                "Reactivar",
                "ClientesBodega",
                $"Se reactivó el cliente {cliente.Nombre}.",
                cliente.Id);

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente reactivado correctamente.",
                Id = id
            };
        }
    }
}
