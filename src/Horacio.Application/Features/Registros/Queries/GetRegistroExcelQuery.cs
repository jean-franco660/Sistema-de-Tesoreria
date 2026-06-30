using Horacio.Application.Common.Interfaces;
using MediatR;

namespace Horacio.Application.Features.Registros.Queries;

/// <summary>Genera el .xlsx oficial del registro de matrícula indicado.</summary>
public record GetRegistroExcelQuery(int RegistroId) : IRequest<byte[]>;

public class GetRegistroExcelQueryHandler : IRequestHandler<GetRegistroExcelQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly IExcelRegistroService _excel;

    public GetRegistroExcelQueryHandler(IMediator mediator, IExcelRegistroService excel)
    {
        _mediator = mediator;
        _excel = excel;
    }

    public async Task<byte[]> Handle(GetRegistroExcelQuery request, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetRegistroRosterQuery(request.RegistroId), ct);
        return _excel.Generar(dto);
    }
}
