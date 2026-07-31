using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.Materiales;
using SistemaFlota.DTOs.Prov_Materiales.Proveedores;
using SistemaFlota.Models.Prov_Materiales.Materiales;

namespace SistemaFlota.Services.Costos.Materiales
{
    public class MaterialService : IMaterialesService
    {
        private readonly AppDbContext _context;

        public MaterialService(AppDbContext context)
        {
            _context = context;
        }

        // Listar Materiales
        public async Task<MaterialPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            string? proveedor,
            string? color,
            int page,
            int pageSize
        )
        {
            var query = _context.Materiales
                .AsNoTracking()
                .Include(m => m.Proveedor)
                .AsQueryable();

            // Buscar por materia prima, proveedor o categoría
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                   m.NombreMaterial.Contains(search) ||
                   (m.Proveedor != null &&
                    m.Proveedor.Nombre.Contains(search)) ||
                   (m.DescripcionCompra != null &&
                    m.DescripcionCompra.Contains(search))
               );
            }

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                query = query.Where(m => m.IdProveedor == int.Parse(proveedor));
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(m => m.Color == color);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                bool activo = estado == "Activo";
                query = query.Where(m =>
                    m.Activo == activo
                );
            }

            query = orden switch
            {
                "precio" => query.OrderBy(m => m.PrecioBaseKg),
                _ => query.OrderBy(m => m.NombreMaterial)
            };

            var totalRegistros = await query.CountAsync();

            var materiales = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MaterialDto
                {
                    IdMaterial = m.IdMaterial,
                    IdProveedor = m.IdProveedor,
                    Proveedor = m.Proveedor != null
                        ? m.Proveedor.Nombre
                        : string.Empty,

                    NombreMaterial = m.NombreMaterial,
                    DescripcionCompra = m.DescripcionCompra,
                    Densidad = m.Densidad,
                    Categoria = m.Categoria,
                    Color = m.Color,
                    TipoProduccion = m.TipoProduccion,
                    Unidad = m.Unidad,
                    PrecioBaseKg = m.PrecioBaseKg,
                    DocumentoPdf = m.DocumentoPdf,
                    Activo = m.Activo,
                    FechaCreacion = m.FechaCreacion,
                    FechaActualizacion = m.FechaActualizacion
                })

                .ToListAsync();


            return new MaterialPaginadoDto
            {
                TotalRegistros = totalRegistros,
                Pagina = page,
                TamanoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(
                    (double)totalRegistros / pageSize
                ),

                Datos = materiales
            };
        }

        public async Task<MaterialDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Materiales
                .AsNoTracking()
                .Include(m => m.Proveedor)
                .Where(m => m.IdMaterial == id)
                .Select(m => new MaterialDto
                {
                    IdMaterial = m.IdMaterial,
                    IdProveedor = m.IdProveedor,
                    Proveedor = m.Proveedor != null
                        ? m.Proveedor.Nombre
                        : string.Empty,
                    NombreMaterial = m.NombreMaterial,
                    DescripcionCompra = m.DescripcionCompra,
                    Densidad = m.Densidad,
                    Categoria = m.Categoria,
                    Color = m.Color,
                    TipoProduccion = m.TipoProduccion,
                    Unidad = m.Unidad,
                    PrecioBaseKg = m.PrecioBaseKg,
                    DocumentoPdf = m.DocumentoPdf,
                    Activo = m.Activo,
                    FechaCreacion = m.FechaCreacion,
                    FechaActualizacion = m.FechaActualizacion
                })
                .FirstOrDefaultAsync();
        }

        // Crear Material
        public async Task<MaterialDto> CrearAsync(CrearMaterialDto dto)
        {

            dto.NombreMaterial = dto.NombreMaterial.Trim();
            dto.DescripcionCompra = dto.DescripcionCompra?.Trim();
            dto.Densidad = dto.Densidad.Trim();
            dto.Categoria = dto.Categoria.Trim();
            dto.Color = dto.Color?.Trim();
            dto.TipoProduccion = dto.TipoProduccion?.Trim();
            dto.Unidad = dto.Unidad.Trim();

            // Validar que exista el proveedor
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p =>
                    p.IdProveedor == dto.IdProveedor &&
                    p.Activo);

            if (proveedor == null)
            {
                throw new InvalidOperationException("El proveedor seleccionado no existe.");
            }

            // Validar que no exista el mismo material para el proveedor
            bool existeMaterial = await _context.Materiales
                .AnyAsync(m =>
                    m.IdProveedor == dto.IdProveedor &&
                    m.NombreMaterial == dto.NombreMaterial);

            if (existeMaterial)
            {
                throw new InvalidOperationException(
                    "Ya existe un material con ese nombre para este proveedor.");
            }

            string? rutaPdf = null;

            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > 0)
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Costos",
                    "Materiales",
                    "FichaTecnica"
                );

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                var nombreArchivo = $"{Guid.NewGuid()}.pdf";

                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);

                await dto.ArchivoPdf.CopyToAsync(stream);

                rutaPdf = $"Costos/Materiales/FichaTecnica/{nombreArchivo}";
            }

            var material = new Material
            {
                IdProveedor = dto.IdProveedor,
                NombreMaterial = dto.NombreMaterial,
                DescripcionCompra = dto.DescripcionCompra,
                Densidad = dto.Densidad,
                Categoria = dto.Categoria,
                Color = dto.Color,
                TipoProduccion = dto.TipoProduccion,
                Unidad = dto.Unidad,
                PrecioBaseKg = dto.PrecioBaseKg,
                DocumentoPdf = rutaPdf,
                Activo = dto.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Materiales.Add(material);

            await _context.SaveChangesAsync();

            return new MaterialDto
            {
                IdMaterial = material.IdMaterial,
                IdProveedor = material.IdProveedor,
                Proveedor = proveedor.Nombre,
                NombreMaterial = material.NombreMaterial,
                DescripcionCompra = material.DescripcionCompra,
                Densidad = material.Densidad,
                Categoria = material.Categoria,
                Color = material.Color,
                TipoProduccion = material.TipoProduccion,
                Unidad = material.Unidad,
                PrecioBaseKg = material.PrecioBaseKg,
                DocumentoPdf = material.DocumentoPdf,
                Activo = material.Activo,
                FechaCreacion = material.FechaCreacion,
                FechaActualizacion = material.FechaActualizacion
            };
        }

        // Para actualizar un registro
        public async Task<bool> ActualizarAsync(int id, ActualizarMaterialDto dto)
        {
            var material = await _context.Materiales
                .FirstOrDefaultAsync(m => m.IdMaterial == id);

            if (material == null)
                return false;


            // Limpiar textos
            dto.NombreMaterial = dto.NombreMaterial.Trim();
            dto.DescripcionCompra = dto.DescripcionCompra?.Trim();
            dto.Densidad = dto.Densidad.Trim();
            dto.Categoria = dto.Categoria.Trim();
            dto.Color = dto.Color?.Trim();
            dto.TipoProduccion = dto.TipoProduccion?.Trim();
            dto.Unidad = dto.Unidad.Trim();


            // Validar proveedor
            bool proveedorExiste = await _context.Proveedores
                .AnyAsync(p =>
                    p.IdProveedor == dto.IdProveedor &&
                    p.Activo);

            if (!proveedorExiste)
            {
                throw new InvalidOperationException(
                    "El proveedor seleccionado no existe o está inactivo."
                );
            }


            // Validar duplicado
            bool existe = await _context.Materiales
                .AnyAsync(m =>
                    m.IdProveedor == dto.IdProveedor &&
                    m.NombreMaterial == dto.NombreMaterial &&
                    m.IdMaterial != id);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Ya existe un material con ese nombre para el proveedor seleccionado."
                );
            }

            // Para actualizar pdf

            string? nuevaRutaPdf = null;

            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > 0)
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Costos",
                    "Materiales",
                    "FichaTecnica"
                );

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                var nombreArchivo = $"{Guid.NewGuid()}.pdf";

                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);

                await dto.ArchivoPdf.CopyToAsync(stream);

                nuevaRutaPdf = $"Costos/Materiales/FichaTecnica/{nombreArchivo}";
            }


            material.IdProveedor = dto.IdProveedor;
            material.NombreMaterial = dto.NombreMaterial;
            material.DescripcionCompra = dto.DescripcionCompra;
            material.Densidad = dto.Densidad;
            material.Categoria = dto.Categoria;
            material.Color = dto.Color;
            material.TipoProduccion = dto.TipoProduccion;
            material.Unidad = dto.Unidad;
            material.PrecioBaseKg = dto.PrecioBaseKg;

            material.Activo = dto.Activo;
            if (nuevaRutaPdf != null)
            {
                if (!string.IsNullOrEmpty(material.DocumentoPdf))
                {
                    var archivoAnterior = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        material.DocumentoPdf
                    );

                    if (File.Exists(archivoAnterior))
                    {
                        File.Delete(archivoAnterior);
                    }
                }

                material.DocumentoPdf = nuevaRutaPdf;
            }

            material.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<FiltrosMaterialDto> ObtenerFiltrosAsync()
        {
            var proveedores = await _context.Materiales
                .Include(m => m.Proveedor)
                .Where(m => m.Proveedor != null)
                .GroupBy(m => new
                {
                    m.IdProveedor,
                    m.Proveedor!.Nombre
                })
                .Select(g => new ProveedorFiltroDto
                {
                    IdProveedor = g.Key.IdProveedor,
                    Nombre = g.Key.Nombre
                })
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var colores = await _context.Materiales
                .Where(m => !string.IsNullOrWhiteSpace(m.Color))
                .Select(m => m.Color!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return new FiltrosMaterialDto
            {
                Proveedores = proveedores,
                Colores = colores
            };
        }

        public Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }
    } 
}
