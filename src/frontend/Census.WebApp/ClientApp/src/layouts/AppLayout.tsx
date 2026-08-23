import { useEffect, useMemo, useState } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { Avatar, Dropdown, Layout, Menu, Typography } from "antd";
import {
  ApartmentOutlined,
  BarChartOutlined,
  LogoutOutlined,
  TeamOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useAuthStore } from "@/features/auth/authStore";
import { CensusLogo } from "@/shared/ui/CensusLogo";
import type { CensusRole } from "@/shared/lib/constants";

const { Sider, Content } = Layout;

type MenuDef = {
  key: string;
  icon: React.ReactNode;
  label: string;
  roles: CensusRole[];
};

const MENU: MenuDef[] = [
  {
    key: "/dashboard",
    icon: <BarChartOutlined />,
    label: "Dashboard",
    roles: ["Analyst", "Admin"],
  },
  {
    key: "/people",
    icon: <TeamOutlined />,
    label: "Pessoas",
    roles: ["Registrar", "Admin"],
  },
  {
    key: "/family-tree",
    icon: <ApartmentOutlined />,
    label: "Árvore genealógica",
    roles: ["Registrar", "Analyst", "Admin"],
  },
  {
    key: "/admin/users",
    icon: <UserOutlined />,
    label: "Usuários",
    roles: ["Admin"],
  },
];

export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);

  const items = useMemo(
    () =>
      MENU.filter((item) => hasAnyRole(item.roles)).map((item) => ({
        key: item.key,
        icon: item.icon,
        label: <Link to={item.key}>{item.label}</Link>,
      })),
    [hasAnyRole]
  );

  const selectedKey =
    MENU.find((m) => location.pathname.startsWith(m.key))?.key ?? "/dashboard";

  useEffect(() => {
    if (!user) return;
    if (location.pathname === "/" || location.pathname === "") {
      if (hasAnyRole(["Analyst", "Admin"])) navigate("/dashboard", { replace: true });
      else if (hasAnyRole(["Registrar"])) navigate("/people", { replace: true });
    }
  }, [user, hasAnyRole, location.pathname, navigate]);

  return (
    <Layout className="census-shell">
      <Sider
        className="census-sider"
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsed}
        width={248}
        theme="dark"
      >
        <div
          className={`census-sider__brand${collapsed ? " census-sider__brand--collapsed" : ""}`}
        >
          <CensusLogo size={40} color="#ffffff" />
          {!collapsed && (
            <Typography.Text className="census-sider__brand-title">
              Censo Demográfico
            </Typography.Text>
          )}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={items}
          className="census-sider__menu"
          style={{ marginTop: 8 }}
        />
        <div className="census-sider__user">
          <Dropdown
            menu={{
              items: [
                {
                  key: "logout",
                  icon: <LogoutOutlined />,
                  label: "Sair",
                  onClick: async () => {
                    await logout();
                    navigate("/login", { replace: true });
                  },
                },
              ],
            }}
            trigger={["click"]}
          >
            <button type="button" className="census-sider__user-btn">
              <Avatar style={{ background: "#2563eb", flexShrink: 0 }} icon={<UserOutlined />} />
              {!collapsed && (
                <span className="census-sider__user-name">
                  {user?.fullName ?? user?.email}
                </span>
              )}
            </button>
          </Dropdown>
        </div>
      </Sider>
      <Layout style={{ minWidth: 0 }}>
        <Content className="census-content">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
