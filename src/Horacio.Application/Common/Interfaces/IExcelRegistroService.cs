using Horacio.Application.Features.Matricula.Queries;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Genera el archivo Excel (.xlsx) del Registro de Matrícula en el formato
/// oficial (a partir de la plantilla con logos y bordes).
/// </summary>
public interface IExcelRegistroService
{
    byte[] Generar(MatriculaDto registro);
}
