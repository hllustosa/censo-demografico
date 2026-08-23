import { useEffect } from "react";
import { ConfigProvider, App as AntApp } from "antd";
import ptBR from "antd/locale/pt_BR";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "react-router-dom";
import { censusTheme } from "@/styles/theme";
import { router } from "@/app/router";
import { useAuthStore } from "@/features/auth/authStore";
import { setUnauthorizedHandler } from "@/shared/api/client";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export function AppProviders() {
  const hydrate = useAuthStore((s) => s.hydrate);

  useEffect(() => {
    hydrate();
    setUnauthorizedHandler(() => {
      if (window.location.pathname !== "/login") {
        window.location.assign("/login");
      }
    });
  }, [hydrate]);

  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={censusTheme} locale={ptBR}>
        <AntApp>
          <RouterProvider router={router} />
        </AntApp>
      </ConfigProvider>
    </QueryClientProvider>
  );
}
