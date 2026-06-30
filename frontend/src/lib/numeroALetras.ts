// Convierte un importe a letras (igual que el backend) para la vista previa en vivo.
// Ej.: 15 -> "QUINCE CON 00/100 SOLES".

const UNIDADES = ["", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE"];
const ESPECIALES = ["DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE"];
const DECENAS = ["", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"];
const CENTENAS = ["", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"];

function decenasUnidades(n: number): string {
  if (n < 10) return UNIDADES[n];
  if (n < 20) return ESPECIALES[n - 10];
  if (n < 30) return n === 20 ? "VEINTE" : "VEINTI" + UNIDADES[n - 20];
  const d = Math.floor(n / 10);
  const u = n % 10;
  return u === 0 ? DECENAS[d] : `${DECENAS[d]} Y ${UNIDADES[u]}`;
}

function grupo(n: number): string {
  if (n === 0) return "";
  if (n === 100) return "CIEN";
  const c = Math.floor(n / 100);
  const resto = n % 100;
  let s = c > 0 ? CENTENAS[c] : "";
  if (resto > 0) s = (s ? s + " " : "") + decenasUnidades(resto);
  return s.trim();
}

function apocopar(g: string): string {
  return g.endsWith("UNO") ? g.slice(0, -1) : g;
}

function convertirEntero(n: number): string {
  if (n === 0) return "CERO";
  const millones = Math.floor(n / 1_000_000);
  const resto = n % 1_000_000;
  const miles = Math.floor(resto / 1000);
  const cientos = resto % 1000;
  const partes: string[] = [];

  if (millones > 0) partes.push(millones === 1 ? "UN MILLON" : apocopar(grupo(millones)) + " MILLONES");
  if (miles > 0) partes.push(miles === 1 ? "MIL" : apocopar(grupo(miles)) + " MIL");
  if (cientos > 0) partes.push(grupo(cientos));

  return partes.join(" ").trim();
}

export function numeroALetras(importe: number): string {
  const v = Math.abs(importe || 0);
  let entero = Math.trunc(v);
  let centavos = Math.round((v - entero) * 100);
  if (centavos === 100) {
    entero += 1;
    centavos = 0;
  }
  return `${convertirEntero(entero)} CON ${String(centavos).padStart(2, "0")}/100 SOLES`;
}
