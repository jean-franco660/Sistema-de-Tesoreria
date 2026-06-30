using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Comprobantes.Commands;

/// <summary>
/// Analiza la FOTO de un comprobante: OCR (Google Vision) → texto → IA (DeepSeek) → JSON.
/// NO guarda nada; solo devuelve los datos extraídos para que el usuario los revise.
/// </summary>
public record AnalizarComprobanteCommand(string ImagenBase64) : IRequest<AnalisisComprobanteDto>;

public class AnalizarComprobanteCommandValidator : AbstractValidator<AnalizarComprobanteCommand>
{
    public AnalizarComprobanteCommandValidator()
    {
        RuleFor(x => x.ImagenBase64).NotEmpty().WithMessage("Debe enviar la imagen del comprobante.");
    }
}

public class AnalizarComprobanteCommandHandler : IRequestHandler<AnalizarComprobanteCommand, AnalisisComprobanteDto>
{
    private readonly IOcrService _ocr;
    private readonly IComprobanteAnalyzer _analyzer;

    public AnalizarComprobanteCommandHandler(IOcrService ocr, IComprobanteAnalyzer analyzer)
    {
        _ocr = ocr;
        _analyzer = analyzer;
    }

    public async Task<AnalisisComprobanteDto> Handle(AnalizarComprobanteCommand request, CancellationToken cancellationToken)
    {
        // 1) OCR: foto → texto plano.
        var texto = await _ocr.ExtraerTextoAsync(request.ImagenBase64, cancellationToken);
        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("No se pudo leer texto en la imagen. Tome la foto más nítida y bien iluminada.");

        // 2) IA: texto → JSON estructurado.
        var analisis = await _analyzer.AnalizarTextoAsync(texto, cancellationToken);
        analisis.TextoOcr = texto;
        return analisis;
    }
}
