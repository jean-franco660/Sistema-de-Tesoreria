import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/Layout";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import TesoreriaPage from "./pages/TesoreriaPage";
import TicketEmitPage from "./pages/TicketEmitPage";
import AlumnosPage from "./pages/AlumnosPage";
import TicketsPage from "./pages/TicketsPage";
import ProgramasPage from "./pages/ProgramasPage";
import ServiciosPage from "./pages/ServiciosPage";
import UsuariosPage from "./pages/UsuariosPage";
import PeriodosPage from "./pages/PeriodosPage";
import MatriculaPage from "./pages/MatriculaPage";
import AuditoriaPage from "./pages/AuditoriaPage";
import ConfiguracionPage from "./pages/ConfiguracionPage";
import ComparativaPage from "./pages/ComparativaPage";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route index element={<TesoreriaPage />} />
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="emitir" element={<TicketEmitPage />} />
              <Route path="alumnos" element={<AlumnosPage />} />
              <Route path="tickets" element={<TicketsPage />} />
              <Route path="programas" element={<ProgramasPage />} />
              <Route path="servicios" element={<ServiciosPage />} />
              <Route path="usuarios" element={<UsuariosPage />} />
              <Route path="periodos" element={<PeriodosPage />} />
              <Route path="comparativa" element={<ComparativaPage />} />
              <Route path="matricula" element={<MatriculaPage />} />
              <Route path="configuracion" element={<ConfiguracionPage />} />
              <Route path="auditoria" element={<AuditoriaPage />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
