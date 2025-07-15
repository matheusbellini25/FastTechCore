using AutoMapper;
using FastTechKitchen.Application.DataTransferObjects;
using FastTechKitchen.Application.Interfaces;
using FastTechKitchen.Domain.Constants;
using FastTechKitchen.Domain.Interfaces;
using System.Linq.Expressions;
using System.Text.Json;
using EN = FastTechKitchen.Domain.Entities;
using MSG = FastTechKitchen.Application.DataTransferObjects.MessageBrokers;

namespace FastTechKitchen.Application.Services;

public class PedidoApplicationService(IPedidoService PedidoService, IMapper mapper) : IPedidoApplicationService
{
    private readonly IPedidoService _PedidoService = PedidoService;
    private readonly IMapper _mapper = mapper;

    public async Task<Pedido> Add(BasicPedido model)
    {
        var Pedido = _mapper.Map<EN.Pedido>(model);

        Pedido = await _PedidoService.Add(Pedido);

        return _mapper.Map<Pedido>(Pedido);
    }

    public async Task<Pedido> Update(Pedido model)
    {
        var Pedido = await _PedidoService.GetById(model.Id, include: false, tracking: true);
        if (Pedido == null)
            throw new Exception("O Item do Cardapio não existe.");

        _mapper.Map(model, Pedido);

        Pedido = await _PedidoService.Update(Pedido);

        return _mapper.Map<Pedido>(Pedido);
    }

    public async Task<Pedido> Add(MSG.BasicPedido model)
    {
        var Pedido = _mapper.Map<EN.Pedido>(model);

        Pedido = await _PedidoService.Add(Pedido);

        return _mapper.Map<Pedido>(Pedido);
    }

    public async Task<IEnumerable<Pedido>> FindBy(Expression<Func<EN.Pedido, bool>> expression)
    {
        var pedidos = _PedidoService.FindBy(expression);
        return _mapper.Map<IEnumerable<Pedido>>(pedidos);
    }


    public async Task<Pedido> Update(MSG.Pedido model)
    {
        var Pedido = await _PedidoService.GetById(model.Id, include: false, tracking: true);
        if (Pedido == null)
            throw new Exception("O Item do Cardapio não existe.");

        _mapper.Map(model, Pedido);

        Pedido = await _PedidoService.Update(Pedido);

        return _mapper.Map<Pedido>(Pedido);
    }

    public async Task<Pedido> GetById(Guid id)
    {
        var Pedido = await _PedidoService.GetById(id, include: false, tracking: false);
        return _mapper.Map<Pedido>(Pedido);
    }

    public async Task Consumer(string message, string rountingKey)
    {
        switch (rountingKey)
        {
            case AppConstants.Routes.RabbitMQ.PedidoInsert:
                var PedidoInsert = JsonSerializer.Deserialize<MSG.BasicPedido>(message);
                await Add(PedidoInsert);
                break;

            case AppConstants.Routes.RabbitMQ.PedidoUpdate:
                var PedidoUpdate = JsonSerializer.Deserialize<MSG.Pedido>(message);
                await Update(PedidoUpdate);
                break;
        }
    }

    public async Task PublishAsync(string message, string rountingKey)
    {
        switch (rountingKey)
        {
            case AppConstants.Routes.RabbitMQ.PedidoInsert:
                var PedidoInsert = JsonSerializer.Deserialize<MSG.BasicPedido>(message);
                await Add(PedidoInsert);
                break;

            case AppConstants.Routes.RabbitMQ.PedidoUpdate:
                var PedidoUpdate = JsonSerializer.Deserialize<MSG.Pedido>(message);
                await Update(PedidoUpdate);
                break;
        }
    }

    public void Dispose()
    {
        _PedidoService.Dispose();

        GC.SuppressFinalize(this);
    }
}
