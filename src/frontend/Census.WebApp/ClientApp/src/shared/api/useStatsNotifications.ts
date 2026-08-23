import { useEffect } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getAccessToken } from "@/shared/api/client";
import { useAuthStore } from "@/features/auth/authStore";

const HUB_URL = "/stats/signair/hubs/notification";

export function useStatsNotifications(enabled = true) {
  const qc = useQueryClient();
  const hasDashboardAccess = useAuthStore((s) =>
    s.hasAnyRole(["Analyst", "Admin"])
  );

  useEffect(() => {
    if (!enabled || !hasDashboardAccess) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => getAccessToken() || "",
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("Notify", () => {
      void qc.invalidateQueries({ queryKey: ["stats"] });
      void qc.invalidateQueries({ queryKey: ["cities"] });
      void qc.invalidateQueries({ queryKey: ["cityCounter"] });
    });

    void connection.start().catch(() => {
      // hub optional when offline
    });

    return () => {
      void connection.stop();
    };
  }, [enabled, hasDashboardAccess, qc]);
}
