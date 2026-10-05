using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Models.ComprasNoFormalizadas.Proveedores;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Proveedores
{
    public class ProveedorNoFormalizadoService : IProveedorNoFormalizadoService
    {
        private readonly AppDbContext _context;

        public ProveedorNoFormalizadoService(AppDbContext context)
        {
            _context = context;
        }

        // Listar proveedores no formalizados
        public async Task<ProveedorNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            int page,
            int pageSize
        )
        {
            var query = _context.ProveedoresNoFormalizados
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.Nombre.Contains(search) ||
                    (p.Documento != null && p.Documento.Contains(search)) ||
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
                "documento" => query.OrderBy(p => p.Documento),
                "contacto" => query.OrderBy(p => p.Contacto),
                _ => query.OrderBy(p => p.Nombre)
            };

            var totalRegistros = await query.CountAsync();

            var proveedores = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProveedorNoFormalizadoDto
                {
                    IdProveedorNoFormalizado = p.IdProveedorNoFormalizado,
                    Nombre = p.Nombre,
                    Documento = p.Documento,
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

            return new ProveedorNoFormalizadoPaginadoDto
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

        // Obtener proveedor por ID
        public async Task<ProveedorNoFormalizadoDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.ProveedoresNoFormalizados
                .AsNoTracking()
                .Where(p => p.IdProveedorNoFormalizado == id)
                .Select(p => new ProveedorNoFormalizadoDto
                {
                    IdProveedorNoFormalizado = p.IdProveedorNoFormalizado,
                    Nombre = p.Nombre,
                    Documento = p.Documento,
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

        // Crear proveedor no formalizado
        public async Task<ProveedorNoFormalizadoDto> CrearAsync(
            CrearProveedorNoFormalizadoDto dto
        )
        {
            dto.Nombre = dto.Nombre.Trim();
            dto.Documento = dto.Documento?.Trim();
            dto.Contacto = dto.Contacto?.Trim();
            dto.Telefono = dto.Telefono?.Trim();
            dto.CorreoElectronico = dto.CorreoElectronico?.Trim();
            dto.Direccion = dto.Direccion?.Trim();
            dto.Ciudad = dto.Ciudad?.Trim();
            dto.Departamento = dto.Departamento?.Trim();

            // Validar documento duplicado
            if (!string.IsNullOrWhiteSpace(dto.Documento))
            {
                bool existeDocumento = await _context.ProveedoresNoFormalizados
                    .AnyAsync(p => p.Documento == dto.Documento);

                if (existeDocumento)
                {
                    throw new InvalidOperationException(
                        "Ya existe un proveedor no formalizado con ese documento."
                    );
                }
            }

            var proveedor = new ProveedorNoFormalizado
            {
                Nombre = dto.Nombre,
                Documento = dto.Documento,
                Contacto = dto.Contacto,
                Telefono = dto.Telefono,
                CorreoElectronico = dto.CorreoElectronico,
                Direccion = dto.Direccion,
                Ciudad = dto.Ciudad,
                Departamento = dto.Departamento,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.ProveedoresNoFormalizados.Add(proveedor);

            await _context.SaveChangesAsync();

            return new ProveedorNoFormalizadoDto
            {
                IdProveedorNoFormalizado = proveedor.IdProveedorNoFormalizado,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Contacto = proveedor.Contacto,
                Telefono = proveedor.Telefono,
                CorreoElectronico = proveedor.CorreoElectronico,
                Direccion = proveedor.Direccion,
                Ciudad = proveedor.Ciudad,
                Departamento = proveedor.Departamento,
                Activo = proveedor.Activo,
                FechaCreacion = proveedor.FechaCreacion,
                FechaActualizacion = proveedor.FechaActualizacion
            };
        }

        // Actualizar proveedor no formalizado
        public async Task<bool> ActualizarAsync(
            int id,
            ActualizarProveedorNoFormalizadoDto dto
        )
        {
            var proveedor = await _context.ProveedoresNoFormalizados
                .FirstOrDefaultAsync(
                    p => p.IdProveedorNoFormalizado == id
                );

            if (proveedor == null)
                return false;

            dto.Nombre = dto.Nombre.Trim();
            dto.Documento = dto.Documento?.Trim();
            dto.Contacto = dto.Contacto?.Trim();
            dto.Telefono = dto.Telefono?.Trim();
            dto.CorreoElectronico = dto.CorreoElectronico?.Trim();
            dto.Direccion = dto.Direccion?.Trim();
            dto.Ciudad = dto.Ciudad?.Trim();
            dto.Departamento = dto.Departamento?.Trim();

            // Validar documento duplicado
            if (!string.IsNullOrWhiteSpace(dto.Documento))
            {
                bool existeDocumento = await _context.ProveedoresNoFormalizados
                    .AnyAsync(p =>
                        p.Documento == dto.Documento &&
                        p.IdProveedorNoFormalizado != id
                    );

                if (existeDocumento)
                {
                    throw new InvalidOperationException(
                        "Ya existe otro proveedor no formalizado con ese documento."
                    );
                }
            }

            proveedor.Nombre = dto.Nombre;
            proveedor.Documento = dto.Documento;
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
    }
}