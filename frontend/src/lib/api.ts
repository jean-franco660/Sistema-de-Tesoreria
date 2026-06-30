import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5080/api",
});

// Adjunta el token JWT en cada petición.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Si el token expira, limpia la sesión y vuelve al login.
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      if (location.pathname !== "/login") location.href = "/login";
    }
    return Promise.reject(error);
  }
);

/** Extrae un mensaje de error legible de una respuesta del backend. */
export function apiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { mensaje?: string; errores?: { error: string }[] } | undefined;
    if (data?.errores?.length) return data.errores.map((e) => e.error).join(" · ");
    if (data?.mensaje) return data.mensaje;
    return error.message;
  }
  return "Ocurrió un error inesperado.";
}

export default api;
