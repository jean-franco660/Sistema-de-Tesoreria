import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/** Formatea un número como soles: 15 -> "S/ 15.00". */
export const soles = (n: number) => `S/ ${Number(n ?? 0).toFixed(2)}`;
