import { Navigate } from "react-router-dom";
import { useAuthStore } from "@/features/auth/authStore";

export function HomeRedirect() {
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);

  if (hasAnyRole(["Analyst", "Admin"])) {
    return <Navigate to="/dashboard" replace />;
  }
  if (hasAnyRole(["Registrar"])) {
    return <Navigate to="/people" replace />;
  }
  return <Navigate to="/login" replace />;
}
