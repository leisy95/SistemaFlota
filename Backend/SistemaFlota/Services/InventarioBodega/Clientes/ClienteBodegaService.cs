
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.Clientes;
using SistemaFlota.Models.InventariosBodega.Clientes;

namespace SistemaFlota.Services.InventarioBodega.Clientes
{
    public class ClienteBodegaService : IClienteBodegaService
    {
        private readonly AppDbContext _context;

        public ClienteBodegaService(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR CLIENTES CON FILTROS Y PAGINACIÓN
        public async Task<ClientesBodegaPaginadosDto> ListarClientesAsync(
            ClienteBodegaFiltroDto filtros)
        {
            int pagina = Math.Max(1, filtros.Pagina);
            int pageSize = Math.Clamp(filtros.PageSize, 1, 100);

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking();

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
                    Email = c.Email
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
                Email = dto.Email?.Trim()
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

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

            await _context.SaveChangesAsync();

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente actualizado correctamente.",
                Id = cliente.Id
            };
        }

        // ELIMINAR CLIENTE
        public async Task<RespuestaClienteDto> EliminarClienteAsync(int id)
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

            // VERIFICAR SI TIENE SALIDAS ASOCIADAS
            var tieneSalidas = await _context.SalidasBodega
                .AnyAsync(s => s.ClienteId == id);

            if (tieneSalidas)
            {
                return new RespuestaClienteDto
                {
                    Exito = false,
                    Mensaje = "No se puede eliminar el cliente porque tiene salidas de bodega asociadas."
                };
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return new RespuestaClienteDto
            {
                Exito = true,
                Mensaje = "Cliente eliminado correctamente.",
                Id = id
            };
        }
    }
}
