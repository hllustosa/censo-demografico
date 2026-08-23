import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "@/layouts/AppLayout";
import { LoginPage } from "@/features/auth/LoginPage";
import { RequireAuth, RequireRoles } from "@/features/auth/RequireAuth";
import { HomeRedirect } from "@/features/auth/HomeRedirect";
import { DashboardPage } from "@/features/dashboard/DashboardPage";
import { PeoplePage } from "@/features/people/PeoplePage";
import { FamilyTreePage } from "@/features/family-tree/FamilyTreePage";
import { UsersPage } from "@/features/users/UsersPage";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    element: <RequireAuth />,
    children: [
      {
        path: "/",
        element: <AppLayout />,
        children: [
          { index: true, element: <HomeRedirect /> },
          {
            element: <RequireRoles roles={["Analyst", "Admin"]} />,
            children: [{ path: "dashboard", element: <DashboardPage /> }],
          },
          {
            element: <RequireRoles roles={["Registrar", "Admin"]} />,
            children: [{ path: "people", element: <PeoplePage /> }],
          },
          {
            element: (
              <RequireRoles roles={["Registrar", "Analyst", "Admin"]} />
            ),
            children: [
              { path: "family-tree", element: <FamilyTreePage /> },
              { path: "family-tree/:personId", element: <FamilyTreePage /> },
            ],
          },
          {
            element: <RequireRoles roles={["Admin"]} />,
            children: [{ path: "admin/users", element: <UsersPage /> }],
          },
        ],
      },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
