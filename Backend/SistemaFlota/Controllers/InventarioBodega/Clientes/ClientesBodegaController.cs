
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.InventarioBodega.Clientes;
using SistemaFlota.Services.InventarioBodega.Clientes;

namespace SistemaFlota.Controllers.InventarioBodega.Clientes
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesBodegaController : ControllerBase
    {
        private readonly IClienteBodegaService _clienteService;

        public ClientesBodegaController(
            IClienteBodegaService clienteService)
        {
            _clienteService = clienteService;
        }

        // LISTAR CLIENTES CON FILTROS Y PAGINACIÓN
        [HttpGet("listar")]
        [Permiso("listar-clientes", "ver")]
        public async Task<IActionResult> ListarClientes(
            [FromQuery] ClienteBodegaFiltroDto filtros)
        {
            var respuesta = await _clienteService.ListarClientesAsync(filtros);

            return Ok(respuesta);
        }

        // LISTAR CLIENTES PARA EL REGISTRO DE SALIDAS
        [HttpGet("para-salidas")]
        [Permiso("salidas", "ver")]
        public async Task<IActionResult> ListarClientesParaSalidas(
            [FromQuery] ClienteBodegaFiltroDto filtros)
        {
            var respuesta = await _clienteService.ListarClientesAsync(filtros);

            return Ok(respuesta);
        }

        // OBTENER CLIENTE POR ID
        [HttpGet("{id:int}")]
        [Permiso("listar-clientes", "ver")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            var respuesta = await _clienteService.ObtenerClienteAsync(id);

            if (respuesta == null)
                return NotFound(new
                {
                    exito = false,
                    mensaje = "Cliente no encontrado."
                });

            return Ok(respuesta);
        }

        // CREAR CLIENTE
        [HttpPost("crear")]
        [Permiso("listar-clientes", "crear")]
        public async Task<IActionResult> CrearCliente(
            [FromBody] ClienteRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var respuesta = await _clienteService.CrearClienteAsync(dto);

            if (!respuesta.Exito)
                return BadRequest(respuesta);

            return Ok(respuesta);
        }

        // ACTUALIZAR CLIENTE
        [HttpPut("actualizar/{id:int}")]
        [Permiso("listar-clientes", "editar")]
        public async Task<IActionResult> ActualizarCliente(
            int id,
            [FromBody] EditarClienteRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var respuesta = await _clienteService.ActualizarClienteAsync(id, dto);

            if (!respuesta.Exito)
                return BadRequest(respuesta);

            return Ok(respuesta);
        }

        /// DESACTIVAR CLIENTE
        [HttpPut("desactivar/{id:int}")]
        [Permiso("listar-clientes", "editar")]
        public async Task<IActionResult> DesactivarCliente(int id)
        {
            var respuesta = await _clienteService.DesactivarClienteAsync(id);

            if (!respuesta.Exito)
                return BadRequest(respuesta);

            return Ok(respuesta);
        }

        // REACTIVAR CLIENTE
        [HttpPut("reactivar/{id:int}")]
        [Permiso("listar-clientes", "editar")]
        public async Task<IActionResult> ReactivarCliente(int id)
        {
            var respuesta = await _clienteService.ReactivarClienteAsync(id);

            if (!respuesta.Exito)
                return BadRequest(respuesta);

            return Ok(respuesta);
        }
    }
}
