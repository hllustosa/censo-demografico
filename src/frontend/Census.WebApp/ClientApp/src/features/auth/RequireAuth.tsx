import { Navigate, Outlet, useLocation } from "react-router-dom";
import { Result, Spin } from "antd";
import { useAuthStore } from "@/features/auth/authStore";
import type { CensusRole } from "@/shared/lib/constants";

export function RequireAuth() {
  const hydrated = useAuthStore((s) => s.hydrated);
  const accessToken = useAuthStore((s) => s.accessToken);
  const location = useLocation();

  if (!hydrated) {
    return (
      <div style={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!accessToken) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}

export function RequireRoles({ roles }: { roles: CensusRole[] }) {
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);

  if (!hasAnyRole(roles)) {
    return (
      <Result
        status="403"
        title="Acesso negado"
        subTitle="Você não tem permissão para acessar esta página."
      />
    );
  }

  return <Outlet />;
}
