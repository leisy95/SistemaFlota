using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Material;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Models.Categorias;
using SistemaFlota.Models.Colores;
using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Materiales
{
    public class MaterialNoFormalizadoService : IMaterialNoFormalizadoService
    {
        private readonly AppDbContext _context;

        public MaterialNoFormalizadoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MaterialNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            string? proveedor,
            string? color,
            int page,
            int pageSize)
        {
            var query = _context.MaterialesNoFormalizados
                .AsNoTracking()
                .Include(m => m.ProveedorNoFormalizado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                    m.NombreMaterial.Contains(search) ||
                    (m.ProveedorNoFormalizado != null &&
                     m.ProveedorNoFormalizado.Nombre.Contains(search)) ||
                    (m.DescripcionCompra != null &&
                     m.DescripcionCompra.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                query = query.Where(m =>
                    m.IdProveedorNoFormalizado == int.Parse(proveedor));
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(m => m.Color == color);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                bool activo = estado == "Activo";
                query = query.Where(m => m.Activo == activo);
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
                .Select(m => new MaterialNoFormalizadoDto
                {
                    IdMaterialNoFormalizado = m.IdMaterialNoFormalizado,
                    IdProveedorNoFormalizado = m.IdProveedorNoFormalizado,
                    Proveedor = m.ProveedorNoFormalizado != null
                        ? m.ProveedorNoFormalizado.Nombre
                        : string.Empty,
                    Codigo = m.Codigo,
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

            return new MaterialNoFormalizadoPaginadoDto
            {
                TotalRegistros = totalRegistros,
                Pagina = page,
                TamanoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(
                    (double)totalRegistros / pageSize),
                Datos = materiales
            };
        }

        public async Task<MaterialNoFormalizadoDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.MaterialesNoFormalizados
                .AsNoTracking()
                .Include(m => m.ProveedorNoFormalizado)
                .Where(m => m.IdMaterialNoFormalizado == id)
                .Select(m => new MaterialNoFormalizadoDto
                {
                    IdMaterialNoFormalizado = m.IdMaterialNoFormalizado,
                    IdProveedorNoFormalizado = m.IdProveedorNoFormalizado,
                    Proveedor = m.ProveedorNoFormalizado != null
                        ? m.ProveedorNoFormalizado.Nombre
                        : string.Empty,
                    Codigo = m.Codigo,
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

        public async Task<MaterialNoFormalizadoDto> CrearAsync(
            CrearMaterialNoFormalizadoDto dto)
        {
            dto.Codigo = dto.Codigo.Trim();
            dto.NombreMaterial = dto.NombreMaterial.Trim();
            dto.DescripcionCompra = dto.DescripcionCompra?.Trim();
            dto.Densidad = dto.Densidad.Trim();
            dto.Categoria = dto.Categoria.Trim();
            dto.Color = dto.Color?.Trim();
            dto.TipoProduccion = dto.TipoProduccion?.Trim();
            dto.Unidad = dto.Unidad.Trim();

            var proveedor = await _context.ProveedoresNoFormalizados
                .FirstOrDefaultAsync(p =>
                    p.IdProveedorNoFormalizado == dto.IdProveedorNoFormalizado &&
                    p.Activo);

            if (proveedor == null)
            {
                throw new InvalidOperationException(
                    "El proveedor seleccionado no existe.");
            }

            bool existeCodigo = await _context.MaterialesNoFormalizados
                .AnyAsync(m => m.Codigo == dto.Codigo);

            if (existeCodigo)
            {
                throw new InvalidOperationException(
                    "Ya existe un material con ese código.");
            }

            bool existeMaterial = await _context.MaterialesNoFormalizados
                .AnyAsync(m =>
                    m.IdProveedorNoFormalizado == dto.IdProveedorNoFormalizado &&
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
                    "ComprasNoFormalizadas",
                    "Materiales",
                    "FichaTecnica");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}.pdf";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(
                    rutaCompleta,
                    FileMode.Create);

                await dto.ArchivoPdf.CopyToAsync(stream);

                rutaPdf =
                    $"ComprasNoFormalizadas/Materiales/FichaTecnica/{nombreArchivo}";
            }

            var material = new MaterialNoFormalizado
            {
                IdProveedorNoFormalizado = dto.IdProveedorNoFormalizado,
                Codigo = dto.Codigo,
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

            _context.MaterialesNoFormalizados.Add(material);
            await _context.SaveChangesAsync();

            return new MaterialNoFormalizadoDto
            {
                IdMaterialNoFormalizado = material.IdMaterialNoFormalizado,
                IdProveedorNoFormalizado =
                    material.IdProveedorNoFormalizado,
                Proveedor = proveedor.Nombre,
                Codigo = material.Codigo,
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

        public async Task<bool> ActualizarAsync(
            int id,
            ActualizarMaterialNoFormalizadoDto dto)
        {
            var material = await _context.MaterialesNoFormalizados
                .FirstOrDefaultAsync(m => m.IdMaterialNoFormalizado == id);

            if (material == null)
                return false;

            dto.Codigo = dto.Codigo.Trim();
            dto.NombreMaterial = dto.NombreMaterial.Trim();
            dto.DescripcionCompra = dto.DescripcionCompra?.Trim();
            dto.Densidad = dto.Densidad.Trim();
            dto.Categoria = dto.Categoria.Trim();
            dto.Color = dto.Color?.Trim();
            dto.TipoProduccion = dto.TipoProduccion?.Trim();
            dto.Unidad = dto.Unidad.Trim();

            bool existeCodigo = await _context.MaterialesNoFormalizados
                .AnyAsync(m =>
                    m.Codigo == dto.Codigo &&
                    m.IdMaterialNoFormalizado != id);

            if (existeCodigo)
            {
                throw new InvalidOperationException(
                    "Ya existe un material con ese código.");
            }

            bool proveedorExiste = await _context.ProveedoresNoFormalizados
                .AnyAsync(p =>
                    p.IdProveedorNoFormalizado == dto.IdProveedorNoFormalizado &&
                    p.Activo);

            if (!proveedorExiste)
            {
                throw new InvalidOperationException(
                    "El proveedor seleccionado no existe o está inactivo.");
            }

            bool existe = await _context.MaterialesNoFormalizados
                .AnyAsync(m =>
                    m.IdProveedorNoFormalizado == dto.IdProveedorNoFormalizado &&
                    m.NombreMaterial == dto.NombreMaterial &&
                    m.IdMaterialNoFormalizado != id);

            if (existe)
            {
                throw new InvalidOperationException(
                    "Ya existe un material con ese nombre para el proveedor seleccionado.");
            }

            string? nuevaRutaPdf = null;

            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > 0)
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "ComprasNoFormalizadas",
                    "Materiales",
                    "FichaTecnica");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}.pdf";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(
                    rutaCompleta,
                    FileMode.Create);

                await dto.ArchivoPdf.CopyToAsync(stream);

                nuevaRutaPdf =
                    $"ComprasNoFormalizadas/Materiales/FichaTecnica/{nombreArchivo}";
            }

            material.IdProveedorNoFormalizado =
                dto.IdProveedorNoFormalizado;
            material.Codigo = dto.Codigo;
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
                        material.DocumentoPdf);

                    if (File.Exists(archivoAnterior))
                        File.Delete(archivoAnterior);
                }

                material.DocumentoPdf = nuevaRutaPdf;
            }

            material.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FiltrosMaterialNoFormalizadoDto> ObtenerFiltrosAsync()
        {
            var proveedores = await _context.MaterialesNoFormalizados
                .Include(m => m.ProveedorNoFormalizado)
                .Where(m => m.ProveedorNoFormalizado != null)
                .GroupBy(m => new
                {
                    m.IdProveedorNoFormalizado,
                    Nombre = m.ProveedorNoFormalizado!.Nombre
                })
                .Select(g => new ProveedorNoFormalizadoFiltroDto
                {
                    IdProveedorNoFormalizado = g.Key.IdProveedorNoFormalizado,
                    Nombre = g.Key.Nombre
                })
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var colores = await _context.MaterialesNoFormalizados
                .Where(m => !string.IsNullOrWhiteSpace(m.Color))
                .Select(m => m.Color!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return new FiltrosMaterialNoFormalizadoDto
            {
                Proveedores = proveedores,
                Colores = colores
            };
        }

        public async Task<List<Color>> ObtenerColoresAsync()
        {
            return await _context.Colores
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}