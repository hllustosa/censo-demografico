import type { ThemeConfig } from "antd";

export const censusTheme: ThemeConfig = {
  token: {
    colorPrimary: "#2563eb",
    colorInfo: "#2563eb",
    colorSuccess: "#15803d",
    colorWarning: "#b45309",
    colorError: "#b91c1c",
    colorBgLayout: "#f1f5f9",
    colorBgContainer: "#ffffff",
    borderRadius: 10,
    fontFamily: '"DM Sans", system-ui, sans-serif',
    fontSize: 14,
    controlHeight: 40,
  },
  components: {
    Layout: {
      headerBg: "#0f172a",
      siderBg: "#1e3a5f",
      triggerBg: "#1e3a5f",
    },
    Menu: {
      darkItemBg: "#1e3a5f",
      darkSubMenuItemBg: "#0f172a",
      darkItemSelectedBg: "#2563eb",
      darkItemHoverBg: "rgba(37, 99, 235, 0.35)",
    },
    Button: {
      primaryShadow: "none",
    },
    Card: {
      borderRadiusLG: 14,
    },
  },
};
