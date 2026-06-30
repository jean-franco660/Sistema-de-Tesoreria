using System.Reflection;
using ClosedXML.Excel;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Matricula.Queries;

namespace Horacio.Infrastructure.Services.Excel;

/// <summary>
/// Genera el Registro de Matrícula en .xlsx usando la plantilla oficial
/// (con logos, bordes y anchos exactos) embebida como recurso. Solo rellena
/// los campos variables y la lista de estudiantes.
/// </summary>
public class ExcelRegistroService : IExcelRegistroService
{
    private const int FilaInicioAlumnos = 16;
    private const int FilaFinPlantilla = 47;   // última fila de alumnos con formato en la plantilla
    private const int Capacidad = FilaFinPlantilla - FilaInicioAlumnos + 1;

    public byte[] Generar(MatriculaDto r)
    {
        using var plantilla = AbrirPlantilla();
        using var wb = new XLWorkbook(plantilla);
        var ws = wb.Worksheets.First();

        // ── Encabezado / título ──
        ws.Cell("A2").Value = $" REGISTRO DE MATRÍCULA {r.Periodo.Replace("-", " - ")}";

        // ── Datos del centro (fijos) ──
        // E5 (Nombre del CETPRO) se deja como en la plantilla oficial (versión corta).
        Set(ws, "L5", r.DreGre);
        Set(ws, "E6", r.CodigoModular);
        Set(ws, "L6", r.Ugel);
        Set(ws, "E7", r.ResolucionCreacion);
        Set(ws, "L7", r.ResolucionAutorizacion);
        Set(ws, "E8", r.Direccion);
        Set(ws, "L8", r.PeriodoLectivo);
        Set(ws, "E11", r.NivelFormativo);
        Set(ws, "L12", r.ModalidadServicio);
        Set(ws, "E12", r.TipoPlan);

        // ── Datos del registro (variables) ──
        Set(ws, "E9", r.Programa);
        Set(ws, "L9", r.ModuloFormativo ?? "");
        Set(ws, "E10", r.Profesor ?? "");
        Set(ws, "L11", r.PeriodoInicio.HasValue ? $"INICIO: {r.PeriodoInicio:dd/MM/yyyy}" : "");
        Set(ws, "E13", r.Turno);
        Set(ws, "L13", $"\"{r.Seccion}\"");

        // ── Estudiantes ──
        var lista = r.Estudiantes;
        if (lista.Count > Capacidad)
        {
            var extra = lista.Count - Capacidad;
            ws.Row(FilaFinPlantilla).InsertRowsBelow(extra);
            for (var fila = FilaFinPlantilla + 1; fila <= FilaFinPlantilla + extra; fila++)
            {
                ws.Range($"B{fila}:D{fila}").Merge();
                ws.Range($"E{fila}:I{fila}").Merge();
            }
        }

        for (var i = 0; i < lista.Count; i++)
        {
            var e = lista[i];
            var fila = FilaInicioAlumnos + i;
            ws.Cell($"A{fila}").Value = (i + 1).ToString("00");
            ws.Cell($"B{fila}").Value = e.CodigoMatricula;
            ws.Cell($"E{fila}").Value = e.ApellidosNombres;
            ws.Cell($"J{fila}").Value = e.Sexo ?? "";
            if (e.FechaNacimiento.HasValue) ws.Cell($"K{fila}").Value = e.FechaNacimiento.Value;
            if (e.Edad.HasValue) ws.Cell($"L{fila}").Value = e.Edad.Value;
            ws.Cell($"N{fila}").Value = 20;
            ws.Cell($"O{fila}").Value = e.Celular ?? "";
        }

        try { ws.Name = SanitizarHoja(r.Turno); } catch { /* nombre inválido o duplicado: se ignora */ }

        using var salida = new MemoryStream();
        wb.SaveAs(salida);
        return salida.ToArray();
    }

    private static void Set(IXLWorksheet ws, string celda, string valor) => ws.Cell(celda).Value = valor ?? "";

    private static string SanitizarHoja(string nombre)
    {
        var limpio = new string((nombre ?? "Registro").Where(c => !"[]:*?/\\".Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(limpio) ? "Registro" : limpio[..Math.Min(31, limpio.Length)];
    }

    private static Stream AbrirPlantilla()
    {
        var asm = Assembly.GetExecutingAssembly();
        var nombre = asm.GetManifestResourceNames().First(n => n.EndsWith("plantilla_registro.xlsx", StringComparison.OrdinalIgnoreCase));
        return asm.GetManifestResourceStream(nombre)!;
    }
}
