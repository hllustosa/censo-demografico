import { useMemo, useState } from "react";
import {
  Button,
  Modal,
  Space,
  Table,
  Tag,
  message,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlusOutlined } from "@ant-design/icons";
import { PageHeader } from "@/shared/ui/PageHeader";
import { useTableBodyScrollY } from "@/shared/ui/useTableBodyScrollY";
import {
  resizableTableComponents,
  useResizableColumns,
} from "@/shared/ui/useResizableColumns";
import {
  useDeactivateUser,
  useUsers,
} from "@/shared/api/hooks";
import type { UserListItem } from "@/shared/api/types";
import { parseApiError } from "@/shared/api/errors";
import { UserFormDrawer } from "./UserFormDrawer";

export function UsersPage() {
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);
  const { data, isLoading } = useUsers(page);
  const deactivateMutation = useDeactivateUser();
  const { containerRef, scrollY } = useTableBodyScrollY();

  const baseColumns: ColumnsType<UserListItem> = useMemo(
    () => [
      { title: "Nome", dataIndex: "fullName", key: "fullName", width: 200, ellipsis: true },
      { title: "E-mail", dataIndex: "email", key: "email", width: 240, ellipsis: true },
      {
        title: "Roles",
        dataIndex: "roles",
        key: "roles",
        width: 200,
        render: (roles: string[]) => (
          <Space wrap>
            {roles.map((role) => (
              <Tag key={role} color={role === "Admin" ? "blue" : "default"}>
                {role}
              </Tag>
            ))}
          </Space>
        ),
      },
      {
        title: "Ativo",
        dataIndex: "isActive",
        key: "isActive",
        width: 100,
        render: (active: boolean) =>
          active ? <Tag color="success">Sim</Tag> : <Tag>Não</Tag>,
      },
      {
        title: "Ações",
        key: "actions",
        width: 120,
        render: (_, record) =>
          record.isActive ? (
            <Button
              danger
              type="link"
              onClick={() => {
                Modal.confirm({
                  title: "Desativar usuário?",
                  content: `O usuário "${record.fullName}" perderá o acesso ao sistema.`,
                  okText: "Desativar",
                  okType: "danger",
                  cancelText: "Cancelar",
                  onOk: async () => {
                    try {
                      await deactivateMutation.mutateAsync(record.id);
                      message.success("Usuário desativado");
                    } catch (err) {
                      message.error(parseApiError(err).message);
                    }
                  },
                });
              }}
            >
              Desativar
            </Button>
          ) : null,
      },
    ],
    [deactivateMutation]
  );

  const { columns, scrollX } = useResizableColumns(baseColumns);

  return (
    <div className="census-table-page">
      <PageHeader
        title="Usuários"
        subtitle="Gestão de contas e papéis de acesso"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setOpen(true)}
          >
            Novo usuário
          </Button>
        }
      />

      <div className="census-table-page__table" ref={containerRef}>
        <Table<UserListItem>
          rowKey="id"
          loading={isLoading && !data}
          components={resizableTableComponents}
          columns={columns}
          dataSource={data?.items ?? []}
          scroll={{ x: scrollX, y: scrollY }}
          pagination={{
            current: page,
            pageSize: 20,
            total: data?.totalItems ?? 0,
            onChange: setPage,
            showSizeChanger: false,
          }}
        />
      </div>

      <UserFormDrawer open={open} onClose={() => setOpen(false)} />
    </div>
  );
}
