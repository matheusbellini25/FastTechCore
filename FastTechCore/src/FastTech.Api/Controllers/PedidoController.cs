﻿using FastTech.Application.DataTransferObjects;
using FastTech.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static FastTech.Domain.Constants.AppConstants;

namespace FastTech.Api.Controllers
{
    [Route("Pedido")]
    public class PedidoController(
        ILogger<PedidoController> logger,
        IPedidoApplicationService pedidoApplicationService,
        IPedidoProducerService pedidoProducerService
    ) : BaseController(logger)
    {
        private readonly IPedidoApplicationService _pedidoApplicationService = pedidoApplicationService;
        private readonly IPedidoProducerService _pedidoProducerService = pedidoProducerService;

        /// <summary>
        /// Criar um novo Pedido
        /// </summary>
        /// <param name="listModel">Objeto com as propriedades para criar um novo Pedido</param>
        /// <returns>Status de envio à fila</returns>
        [HttpPost]
        [Authorize(Policy = Policies.Cliente)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] List<BasicPedido> listModel)
        {
            try
            {
                // Publicar os pedidos na fila
                string jsonMessage = JsonSerializer.Serialize(listModel);

                await _pedidoProducerService.PublishMessageAsync(jsonMessage);

                return Ok(new { message = "Pedidos enviados para a fila com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
