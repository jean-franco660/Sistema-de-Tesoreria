package pe.edu.hzg.tesoreria.data.model;

import java.util.List;

/** Egreso devuelto por POST /api/comprobantes (ComprobanteDto del backend). */
public class ComprobanteResponse {
    public int id;
    public String proveedor;
    public String ruc;
    public String tipoDocumento;
    public String numeroComprobante;
    public String fechaEmision;   // ISO (puede ser null)
    public String horaEmision;
    public String moneda;
    public Double subtotal;
    public Double igv;
    public double total;
    public String categoria;
    public String concepto;
    public String metodoPago;
    public int confianza;
    public boolean esDuplicadoProbable;
    public String observaciones;
    public String imagenUrl;
    public String estado;
    public List<Producto> productos;

    public static class Producto {
        public String descripcion;
        public Double cantidad;
        public Double precioUnitario;
        public Double importe;
    }
}
