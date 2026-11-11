
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.SalidasBodega;
using SistemaFlota.Models.InventariosBodega;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using SistemaFlota.Models.InventariosBodega.SalidasBodega;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.InventarioBodega.SalidasBodega
{
    public class SalidaBodegaService : BaseService, ISalidaBodegaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        private string UsuarioActual =>
            _currentUser.Usuario ?? "Sistema";

        public SalidaBodegaService(
            AppDbContext context,
            ICurrentUserService currentUser,
            AuditoriaService auditoria
        ) : base(auditoria, currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // OBTENER SALIDA ACTUAL
        public async Task<SalidaBodegaActualDto> ObtenerSalidaActualAsync()
        {
            var salida = await _context.SalidasBodega
                .AsNoTracking()
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.InventarioBodegaItem)
                .FirstOrDefaultAsync(s =>
                    s.Usuario == UsuarioActual &&
                    s.Estado == "EN_PROCESO"
                );

            if (salida == null)
            {
                return new SalidaBodegaActualDto
                {
                    Existe = false,
                    Mensaje = "No hay salida en proceso",
                    Detalles = new List<SalidaBodegaDetalleDto>()
                };
            }

            return new SalidaBodegaActualDto
            {
                Existe = true,
                Id = salida.Id,
                Estado = salida.Estado,
                FechaCreacion = salida.FechaCreacion,
                Detalles = salida.Detalles.Select(d =>
                    new SalidaBodegaDetalleDto
                    {
                        IdInventarioBodegaItem =
                            d.InventarioBodegaItemId,

                        ReferenciaPeso =
                            d.InventarioBodegaItem.ReferenciaPeso,

                        PesoActual =
                            d.InventarioBodegaItem.PesoActual
                    }).ToList()
            };
        }

        // CREAR SALIDA
        public async Task<RespuestaSalidaBodegaDto> CrearSalidaAsync()
        {
            var existente = await _context.SalidasBodega
                .FirstOrDefaultAsync(s =>
                    s.Usuario == UsuarioActual &&
                    s.Estado == "EN_PROCESO"
                );

            if (existente != null)
            {
                return new RespuestaSalidaBodegaDto
                {
                    Exito = false,
                    Mensaje = "Ya tienes una salida en proceso.",
                    Id = existente.Id
                };
            }

            var salida = new SalidaBodega
            {
                Usuario = UsuarioActual,
                Estado = "EN_PROCESO",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SalidasBodega.Add(salida);

            await _context.SaveChangesAsync();

            return new RespuestaSalidaBodegaDto
            {
                Exito = true,
                Mensaje = "Salida creada correctamente.",
                Id = salida.Id
            };
        }

        // AGREGAR ITEM POR ID
        public async Task<RespuestaSalidaBodegaDto> AgregarItemAsync(
            int idSalida,
            int idInventarioBodegaItem)
        {
            var salida = await _context.SalidasBodega
                .Include(s => s.Detalles)
                .FirstOrDefaultAsync(s =>
                    s.Id == idSalida &&
                    s.Estado == "EN_PROCESO"
                );

            if (salida == null)
            {
                return Error("Salida no encontrada o ya finalizada.");
            }

            var item = await _context.InventariosBodegaItems
                .FirstOrDefaultAsync(i =>
                    i.Id == idInventarioBodegaItem &&
                    (i.Estado == "EN_ALMACEN" ||
                     i.Estado == "RESERVADO")
                );

            if (item == null)
            {
                return Error("El item no está disponible en almacén.");
            }

            var reservadoEnOtraSalida =
                await _context.SalidasBodegaDetalles
                    .AnyAsync(d =>
                        d.InventarioBodegaItemId == item.Id &&
                        d.SalidaBodega.Estado == "EN_PROCESO" &&
                        d.SalidaBodegaId != idSalida
                    );

            if (reservadoEnOtraSalida)
            {
                return Error("El item ya está reservado en otra salida.");
            }

            bool yaAgregado = salida.Detalles.Any(d =>
                d.InventarioBodegaItemId == item.Id
            );

            if (yaAgregado)
            {
                return Error("El item ya estaba en esta salida.");
            }

            item.Estado = "RESERVADO";

            _context.SalidasBodegaDetalles.Add(
                new SalidaBodegaDetalle
                {
                    SalidaBodegaId = salida.Id,
                    InventarioBodegaItemId = item.Id,
                    FechaAgregado = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            return Exito("Item agregado a la salida.", item.Id);
        }

        // AGREGAR ITEM POR REFERENCIA
        public async Task<RespuestaSalidaBodegaDto>
            AgregarItemPorReferenciaAsync(
                int idSalida,
                string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
            {
                return Error("Debes ingresar una referencia.");
            }

            referencia = referencia.Trim().ToUpperInvariant();

            var salida = await _context.SalidasBodega
                .Include(s => s.Detalles)
                .FirstOrDefaultAsync(s =>
                    s.Id == idSalida &&
                    s.Estado == "EN_PROCESO"
                );

            if (salida == null)
            {
                return Error("Salida no encontrada o ya finalizada.");
            }

            var item = await _context.InventariosBodegaItems
                .Where(i =>
                    i.ReferenciaPeso == referencia &&
                    i.Estado == "EN_ALMACEN"
                )
                .OrderBy(i => i.FechaRegistroItem)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return Error(
                    "No hay items disponibles con esa referencia."
                );
            }

            bool yaAgregado = salida.Detalles.Any(d =>
                d.InventarioBodegaItemId == item.Id
            );

            if (yaAgregado)
            {
                return Error("El item ya estaba en esta salida.");
            }

            item.Estado = "RESERVADO";

            _context.SalidasBodegaDetalles.Add(
                new SalidaBodegaDetalle
                {
                    SalidaBodegaId = salida.Id,
                    InventarioBodegaItemId = item.Id,
                    FechaAgregado = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            return Exito("Item agregado correctamente.", item.Id);
        }

        // QUITAR ITEM
        public async Task<RespuestaSalidaBodegaDto> QuitarItemAsync(
            int idSalida,
            int idInventarioBodegaItem)
        {
            var salidaItem = await _context.SalidasBodegaDetalles
                .Include(d => d.InventarioBodegaItem)
                .Include(d => d.SalidaBodega)
                .FirstOrDefaultAsync(d =>
                    d.SalidaBodegaId == idSalida &&
                    d.InventarioBodegaItemId ==
                        idInventarioBodegaItem &&
                    d.SalidaBodega.Estado == "EN_PROCESO"
                );

            if (salidaItem == null)
            {
                return Error("El item no está en esta salida.");
            }

            salidaItem.InventarioBodegaItem.Estado = "EN_ALMACEN";

            _context.SalidasBodegaDetalles.Remove(salidaItem);

            await _context.SaveChangesAsync();

            return Exito("Item eliminado de la salida.");
        }

        // CONFIRMAR SALIDA
        public async Task<RespuestaSalidaBodegaDto> ConfirmarSalidaAsync(
            int idSalida,
            ConfirmarSalidaBodegaDto dto)
        {
            var salida = await _context.SalidasBodega
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.InventarioBodegaItem)
                        .ThenInclude(i => i.InventarioBodega)
                .FirstOrDefaultAsync(s =>
                    s.Id == idSalida &&
                    s.Estado == "EN_PROCESO"
                );

            if (salida == null)
            {
                return Error("No se encontró la salida.");
            }

            if (!dto.ClienteId.HasValue)
            {
                return Error("Debes seleccionar un cliente.");
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == dto.ClienteId.Value);

            if (cliente == null)
            {
                return Error("Cliente no encontrado.");
            }

            if (!salida.Detalles.Any())
            {
                return Error("No puedes confirmar una salida sin items.");
            }

            salida.ClienteId = dto.ClienteId;

            foreach (var detalle in salida.Detalles)
            {
                var item = detalle.InventarioBodegaItem;

                if (item.Estado != "RESERVADO")
                {
                    return Error(
                        $"El item {item.ReferenciaPeso} ya no está reservado."
                    );
                }

                var inventario = item.InventarioBodega;

                if (inventario == null)
                {
                    return Error(
                        $"No se encontró el inventario del item {item.ReferenciaPeso}."
                    );
                }

                if (inventario.Cantidad < 1 ||
                    inventario.Peso < item.PesoActual)
                {
                    return Error(
                        $"Inventario insuficiente para el item {item.ReferenciaPeso}."
                    );
                }
            }

            foreach (var detalle in salida.Detalles)
            {
                var item = detalle.InventarioBodegaItem;
                var inventario = item.InventarioBodega;

                item.Estado = "SALIDA";

                _context.MovimientosInventarioBodega.Add(
                    new MovimientoInventarioBodega
                    {
                        Referencia =
                            inventario.ReferenciaNormalizada ??
                            inventario.Referencia,

                        ReferenciaPeso = item.ReferenciaPeso,
                        Peso = item.PesoActual,
                        Fecha = DateTime.UtcNow,
                        Tipo = "Salida",
                        Usuario = UsuarioActual,
                        Observacion = $"Salida a cliente: {cliente.Nombre}",
                        IdInventarioBodega = item.IdInventarioBodega,
                        IdInventarioBodegaItem = item.Id
                    }
                );

                inventario.Peso -= item.PesoActual;
                inventario.Cantidad -= 1;
            }

            salida.Estado = "CONFIRMADA";
            salida.FechaSalida = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Exito("Salida confirmada correctamente.", salida.Id);
        }

        // CANCELAR SALIDA
        public async Task<RespuestaSalidaBodegaDto> CancelarSalidaAsync(
            int idSalida)
        {
            var salida = await _context.SalidasBodega
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.InventarioBodegaItem)
                .FirstOrDefaultAsync(s =>
                    s.Id == idSalida &&
                    s.Estado == "EN_PROCESO"
                );

            if (salida == null)
            {
                return Error("Salida no válida.");
            }

            foreach (var detalle in salida.Detalles)
            {
                detalle.InventarioBodegaItem.Estado = "EN_ALMACEN";
            }

            salida.Estado = "CANCELADA";
            salida.FechaCancelacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Exito("Salida cancelada correctamente.", salida.Id);
        }

        // RESPUESTAS
        private static RespuestaSalidaBodegaDto Error(string mensaje)
        {
            return new RespuestaSalidaBodegaDto
            {
                Exito = false,
                Mensaje = mensaje
            };
        }

        private static RespuestaSalidaBodegaDto Exito(
            string mensaje,
            int? id = null)
        {
            return new RespuestaSalidaBodegaDto
            {
                Exito = true,
                Mensaje = mensaje,
                Id = id
            };
        }
    }
}
