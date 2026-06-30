import { createContext, useContext, useState, type ReactNode } from "react";
import api from "../lib/api";
import type { AuthResponse } from "../types";

interface UserInfo {
  username: string;
  nombreCompleto: string;
  rol: string;
}

interface AuthContextType {
  token: string | null;
  user: UserInfo | null;
  isAdmin: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("token"));
  const [user, setUser] = useState<UserInfo | null>(() => {
    const raw = localStorage.getItem("user");
    return raw ? (JSON.parse(raw) as UserInfo) : null;
  });

  const login = async (username: string, password: string) => {
    const { data } = await api.post<AuthResponse>("/auth/login", { username, password });
    const info: UserInfo = {
      username: data.username,
      nombreCompleto: data.nombreCompleto,
      rol: data.rol,
    };
    localStorage.setItem("token", data.token);
    localStorage.setItem("user", JSON.stringify(info));
    setToken(data.token);
    setUser(info);
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ token, user, isAdmin: user?.rol === "Administrador", login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth debe usarse dentro de <AuthProvider>");
  return ctx;
}
