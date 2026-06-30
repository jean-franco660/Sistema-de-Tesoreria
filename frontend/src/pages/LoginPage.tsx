import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { apiError } from "../lib/api";
import { useConfig, logoSrc } from "../lib/useConfig";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";

export default function LoginPage() {
  const { login } = useAuth();
  const config = useConfig();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login(username, password);
      navigate("/");
    } catch (err) {
      setError(apiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex h-full items-center justify-center bg-gradient-to-br from-sidebar via-sidebar to-[#101728] p-4">
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-xl">
        <div className="mb-6 text-center">
          <img src={logoSrc(config)} alt="Logo institucional" className="mx-auto mb-3 h-24 w-auto max-h-24 object-contain" />
          <h1 className="text-base font-semibold leading-snug text-slate-800">
            {config?.nombreInstitucion || 'I.E.S.T.P. "Horacio Zeballos Gámez"'}
          </h1>
          <p className="mt-1 text-sm text-slate-500">Sistema de Tesorería · Recursos Propios</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <Label htmlFor="username">Usuario</Label>
            <Input id="username" value={username} onChange={(e) => setUsername(e.target.value)} autoFocus required />
          </div>
          <div>
            <Label htmlFor="password">Contraseña</Label>
            <Input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </div>

          {error && <p className="rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{error}</p>}

          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? "Ingresando…" : "Ingresar"}
          </Button>
        </form>

        <p className="mt-4 text-center text-xs text-slate-400">
          Comprobante interno · No es facturación electrónica
        </p>
      </div>
    </div>
  );
}
