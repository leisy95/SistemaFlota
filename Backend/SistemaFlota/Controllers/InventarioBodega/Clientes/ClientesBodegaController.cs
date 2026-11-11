
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> ListarClientes(
            [FromQuery] ClienteBodegaFiltroDto filtros)
        {
            var respuesta = await _clienteService.ListarClientesAsync(filtros);

            return Ok(respuesta);
        }

        // OBTENER CLIENTE POR ID
        [HttpGet("{id:int}")]
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

        // ELIMINAR CLIENTE
        [HttpDelete("eliminar/{id:int}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var respuesta = await _clienteService.EliminarClienteAsync(id);

            if (!respuesta.Exito)
                return BadRequest(respuesta);

            return Ok(respuesta);
        }
    }
}
