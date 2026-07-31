using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Prov_Materiales.Proveedores;
using SistemaFlota.Models.Proveedores;

namespace SistemaFlota.Services.Costos.Proveedores
{
    public class ProveedorService : IProveedorService
    {
        private readonly AppDbContext _context;

        public ProveedorService(AppDbContext context)
        {
            _context = context;
        }

        // Listar Proveedores
        public async Task<ProveedorPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            int page,
            int pageSize
        )
        {
            var query = _context.Proveedores
                   .AsNoTracking()
                   .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                     p.Nombre.Contains(search) ||
                     p.Nit.Contains(search) ||
                     (p.Contacto != null && p.Contacto.Contains(search)) ||
                     (p.Telefono != null && p.Telefono.Contains(search)) ||
                     (p.CorreoElectronico != null && p.CorreoElectronico.Contains(search)) ||
                     (p.Direccion != null && p.Direccion.Contains(search)) ||
                     (p.Ciudad != null && p.Ciudad.Contains(search)) ||
                     (p.Departamento != null && p.Departamento.Contains(search))
                 );
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                bool activo = estado == "Activo";
                query = query.Where(p =>
                    p.Activo == activo
                );
            }

            query = orden switch
            {
                "nit" => query.OrderBy(p => p.Nit),
                "contacto" => query.OrderBy(p => p.Contacto),
                _ => query.OrderBy(p => p.Nombre)
            };


            var totalRegistros = await query.CountAsync();

            var proveedores = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProveedorDto
                {
                    IdProveedor = p.IdProveedor,
                    Nombre = p.Nombre,
                    Nit = p.Nit,
                    Contacto = p.Contacto,
                    Telefono = p.Telefono,
                    CorreoElectronico = p.CorreoElectronico,
                    Direccion = p.Direccion,
                    Ciudad = p.Ciudad,
                    Departamento = p.Departamento,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion
                })
                .ToListAsync();

            return new ProveedorPaginadoDto
            {
                TotalRegistros = totalRegistros,
                Pagina = page,
                TamanoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(
                    (double)totalRegistros / pageSize
                ),
                Datos = proveedores
            };
        }

        // Para editar trae los datos de la bd
        public async Task<ProveedorDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedores
                .Where(p => p.IdProveedor == id)
                .Select(p => new ProveedorDto
                {
                    IdProveedor = p.IdProveedor,
                    Nombre = p.Nombre,
                    Nit = p.Nit,
                    Contacto = p.Contacto,
                    Telefono = p.Telefono,
                    CorreoElectronico = p.CorreoElectronico,
                    Direccion = p.Direccion,
                    Ciudad = p.Ciudad,
                    Departamento = p.Departamento,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion
                })
                .FirstOrDefaultAsync();
        }

        // Crear Proveedor
        public async Task<ProveedorDto> CrearAsync(CrearProveedorDto dto)
        {
            dto.Nombre = dto.Nombre.Trim();
            dto.Nit = dto.Nit.Trim();
            dto.Contacto = dto.Contacto?.Trim();
            dto.Telefono = dto.Telefono?.Trim();
            dto.CorreoElectronico = dto.CorreoElectronico?.Trim();
            dto.Direccion = dto.Direccion?.Trim();
            dto.Ciudad = dto.Ciudad?.Trim();
            dto.Departamento = dto.Departamento?.Trim();

            // Validar que no exista un proveedor con el mismo NIT
            bool existeNit = await _context.Proveedores
                .AnyAsync(p => p.Nit == dto.Nit);

            if (existeNit)
            {
                throw new InvalidOperationException("Ya existe un proveedor con ese NIT.");
            }

            var proveedor = new Proveedor
            {
                Nombre = dto.Nombre,
                Nit = dto.Nit,
                Contacto = dto.Contacto,
                Telefono = dto.Telefono,
                CorreoElectronico = dto.CorreoElectronico,
                Direccion = dto.Direccion,
                Ciudad = dto.Ciudad,
                Departamento = dto.Departamento,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Proveedores.Add(proveedor);

            await _context.SaveChangesAsync();

            return new ProveedorDto
            {
                IdProveedor = proveedor.IdProveedor,
                Nombre = proveedor.Nombre,
                Nit = proveedor.Nit,
                Contacto = proveedor.Contacto,
                Telefono = proveedor.Telefono,
                CorreoElectronico = proveedor.CorreoElectronico,
                Activo = proveedor.Activo,
                FechaCreacion = proveedor.FechaCreacion,
                FechaActualizacion = proveedor.FechaActualizacion
            };
        }

        //Para actualizar un registro de proveedor
        public async Task<bool> ActualizarAsync(int id, ActualizarProveedorDto dto)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id);

            if (proveedor == null)
                return false;


            dto.Nombre = dto.Nombre.Trim();
            dto.Nit = dto.Nit.Trim();
            dto.Contacto = dto.Contacto?.Trim();
            dto.Telefono = dto.Telefono?.Trim();
            dto.CorreoElectronico = dto.CorreoElectronico?.Trim();
            dto.Direccion = dto.Direccion?.Trim();
            dto.Ciudad = dto.Ciudad?.Trim();
            dto.Departamento = dto.Departamento?.Trim();


            // Validar NIT duplicado
            bool existeNit = await _context.Proveedores
                .AnyAsync(p =>
                    p.Nit == dto.Nit &&
                    p.IdProveedor != id
                );

            if (existeNit)
            {
                throw new InvalidOperationException(
                    "Ya existe otro proveedor con ese NIT."
                );
            }


            proveedor.Nombre = dto.Nombre;
            proveedor.Nit = dto.Nit;
            proveedor.Contacto = dto.Contacto;
            proveedor.Telefono = dto.Telefono;
            proveedor.CorreoElectronico = dto.CorreoElectronico;
            proveedor.Direccion = dto.Direccion;
            proveedor.Ciudad = dto.Ciudad;
            proveedor.Departamento = dto.Departamento;
            proveedor.FechaActualizacion = DateTime.UtcNow;


            await _context.SaveChangesAsync();

            return true;
        }

        public Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProveedorPaginadoDto> ObtenerAsync(string? search, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}