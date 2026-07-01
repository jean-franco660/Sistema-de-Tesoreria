using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Horacio.Persistence.Seed;

/// <summary>
/// Carga inicial de datos (seeders). Idempotente: solo inserta si la tabla está vacía.
/// </summary>
public static class DbInitializer
{
    private static readonly string[] ProgramasIniciales =
    {
        "SOPORTE TECNICO Y OPERACION DE CENTROS DE COMPUTO",
        "PANADERIA Y PASTELERIA",
        "APOYO ADMINISTRATIVO",
        "CORTE Y ENSAMBLAJE",
        "PRODUCCION DE TEJEDURIA",
        "PELUQUERIA Y BARBERIA",
        "MANTENIMIENTO DE SISTEMAS ELECTRONICOS",
        "FABRICACION ARTESANAL DE PRODUCTOS DE MADERA",
        "MECANICA DE MOTOS Y VEHICULOS AFINES",
        "DIBUJO TECNICO MECANICO",
        "GASTRONOMIA"
    };

    private static readonly Dictionary<string, decimal> ServiciosIniciales = new()
    {
        ["Mantenimiento, equipamiento, infraestructura y otros"] = 5.00m,
        ["Certificado de Capacitación Modular"] = 15.00m,
        ["Constancias"] = 10.00m,
        ["Certificado de Estudios"] = 20.00m,
        ["Expedición de Título"] = 50.00m,
        ["Carnet del estudiante"] = 8.00m,
        ["Folder y/o Medallas"] = 12.00m,
        ["Examen teórico y práctico y/o curso de nivelación"] = 25.00m,
        ["Proyectos productivos y/o donación"] = 25.00m,
        ["FUT"] = 8.00m,
        ["Otros"] = 5.00m
    };

    private static readonly string[] TurnosIniciales = { "MAÑANA", "TARDE", "NOCHE" };

    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (!await context.Turnos.AnyAsync(ct))
            context.Turnos.AddRange(TurnosIniciales.Select(n => new Turno { Nombre = n }));

        if (!await context.Programas.AnyAsync(ct))
            context.Programas.AddRange(ProgramasIniciales.Select(n => new Programa { Nombre = n }));

        if (!await context.Servicios.AnyAsync(ct))
            context.Servicios.AddRange(ServiciosIniciales.Select(kv => new Servicio { Nombre = kv.Key, Precio = kv.Value }));

        if (!await context.Configuraciones.AnyAsync(ct))
            context.Configuraciones.Add(new Configuracion
            {
                NombreInstitucion = "INSTITUTO DE EDUCACIÓN SUPERIOR TECNOLÓGICO PRODUCTIVO \"HORACIO ZEBALLOS GÁMEZ\"",
                Ciudad = "JULIACA — PUNO, PERÚ",
                CodigoModular = "240010",
                Direccion = "JR. LIMA 430",
                BaseLegal = "D.S. N° 028-2007-ED",
                TituloComprobante = "INGRESO POR RECURSOS PROPIOS Y ACTIVIDADES PRODUCTIVAS",
                TipoComprobante = "Interno (no SUNAT)",
                DreGre = "PUNO",
                Ugel = "SAN ROMÁN",
                ResolucionCreacion = "R.M. N° 0285-2005-ED / R.D. N° 0375-2005-DREP",
                ResolucionAutorizacion = "R.D.N° 1963-2022-UGEL-SR",
                PeriodoLectivo = "2026",
                ModalidadServicio = "PRESENCIAL",
                NivelFormativo = "CICLO AUXILIAR TÉCNICO",
                TipoPlan = "POR COMPETENCIAS"
            });

        if (!await context.PeriodosAcademicos.AnyAsync(ct))
            context.PeriodosAcademicos.Add(new PeriodoAcademico
            {
                Nombre = "2026-I",
                FechaInicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc),
                Estado = EstadoPeriodo.Abierto,
                UsuarioApertura = "admin",
                Observaciones = "Período inicial del sistema."
            });

        if (!await context.Contadores.AnyAsync(ct))
            context.Contadores.AddRange(
                new Contador { Nombre = "GENERAL", Serie = "000", UltimoValor = 0 },
                new Contador { Nombre = "TICKET", Serie = "001", UltimoValor = 0 });

        if (!await context.Usuarios.AnyAsync(u => u.Username.ToLower() == "admin", ct))
        {
            context.Usuarios.Add(new Usuario
            {
                Username = "admin",
                NombreCompleto = "Administrador del Sistema",
                PasswordHash = hasher.Hash("Admin123*"),
                Rol = RolUsuario.Administrador,
                Estado = EstadoRegistro.Activo
            });
        }

        await context.SaveChangesAsync(ct);

        // Backfill: asigna precio a los servicios sembrados que aún estén en 0.
        var sinPrecio = await context.Servicios.Where(s => s.Precio == 0).ToListAsync(ct);
        var cambios = false;
        foreach (var s in sinPrecio)
        {
            if (ServiciosIniciales.TryGetValue(s.Nombre, out var precio))
            {
                s.Precio = precio;
                cambios = true;
            }
        }
        if (cambios) await context.SaveChangesAsync(ct);

        // Backfill: período académico activo + sección por defecto para alumnos existentes.
        var activo = await context.PeriodosAcademicos.FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);
        var aArreglar = await context.Alumnos
            .Where(a => a.PeriodoAcademicoId == null || a.Seccion == null || a.Seccion == "")
            .ToListAsync(ct);
        foreach (var a in aArreglar)
        {
            if (a.PeriodoAcademicoId == null && activo is not null) a.PeriodoAcademicoId = activo.Id;
            if (string.IsNullOrEmpty(a.Seccion)) a.Seccion = "U";
        }
        if (aArreglar.Count > 0) await context.SaveChangesAsync(ct);

        // Backfill: crea un Registro de Matrícula por cada combinación de alumnos
        // ya existente (período + programa + turno + sección) que aún no lo tenga.
        var registrosExistentes = (await context.RegistrosMatricula.ToListAsync(ct))
            .Select(r => (r.PeriodoAcademicoId, r.ProgramaId, r.TurnoId, r.Seccion))
            .ToHashSet();

        var combosAlumnos = (await context.Alumnos
                .Where(a => a.PeriodoAcademicoId != null)
                .Select(a => new { Periodo = a.PeriodoAcademicoId!.Value, a.ProgramaId, a.TurnoId, a.Seccion })
                .Distinct()
                .ToListAsync(ct));

        var nuevosRegistros = combosAlumnos
            .Where(c => !registrosExistentes.Contains((c.Periodo, c.ProgramaId, c.TurnoId, c.Seccion)))
            .Select(c => new RegistroMatricula
            {
                PeriodoAcademicoId = c.Periodo,
                ProgramaId = c.ProgramaId,
                TurnoId = c.TurnoId,
                Seccion = string.IsNullOrEmpty(c.Seccion) ? "U" : c.Seccion
            })
            .ToList();

        if (nuevosRegistros.Count > 0)
        {
            context.RegistrosMatricula.AddRange(nuevosRegistros);
            await context.SaveChangesAsync(ct);
        }

        // Backfill: completa los datos fijos del registro de matrícula en la
        // configuración existente si aún están vacíos.
        var cfg = await context.Configuraciones.FirstOrDefaultAsync(ct);
        if (cfg is not null)
        {
            var cambioCfg = false;
            void Set(Func<string> get, Action<string> set, string val) { if (string.IsNullOrWhiteSpace(get())) { set(val); cambioCfg = true; } }
            Set(() => cfg.DreGre, v => cfg.DreGre = v, "PUNO");
            Set(() => cfg.Ugel, v => cfg.Ugel = v, "SAN ROMÁN");
            Set(() => cfg.ResolucionCreacion, v => cfg.ResolucionCreacion = v, "R.M. N° 0285-2005-ED / R.D. N° 0375-2005-DREP");
            Set(() => cfg.ResolucionAutorizacion, v => cfg.ResolucionAutorizacion = v, "R.D.N° 1963-2022-UGEL-SR");
            Set(() => cfg.PeriodoLectivo, v => cfg.PeriodoLectivo = v, "2026");
            Set(() => cfg.ModalidadServicio, v => cfg.ModalidadServicio = v, "PRESENCIAL");
            Set(() => cfg.NivelFormativo, v => cfg.NivelFormativo = v, "CICLO AUXILIAR TÉCNICO");
            Set(() => cfg.TipoPlan, v => cfg.TipoPlan = v, "POR COMPETENCIAS");
            if (cambioCfg) await context.SaveChangesAsync(ct);
        }
    }
}
