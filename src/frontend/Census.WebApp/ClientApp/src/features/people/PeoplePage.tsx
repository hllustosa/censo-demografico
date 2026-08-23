import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Button,
  Input,
  Modal,
  Space,
  Table,
  Tag,
  Tooltip,
  message,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import {
  ApartmentOutlined,
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import { PageHeader } from "@/shared/ui/PageHeader";
import { useTableBodyScrollY } from "@/shared/ui/useTableBodyScrollY";
import {
  resizableTableComponents,
  useResizableColumns,
} from "@/shared/ui/useResizableColumns";
import { useDeletePerson, usePeople } from "@/shared/api/hooks";
import type { Person } from "@/shared/api/types";
import { sexLabel } from "@/shared/lib/constants";
import { parseApiError } from "@/shared/api/errors";
import { PersonFormDrawer } from "./PersonFormDrawer";

export function PeoplePage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<Person | null>(null);
  const { containerRef, scrollY } = useTableBodyScrollY();

  const { data, isLoading } = usePeople(page, search);
  const deleteMutation = useDeletePerson();

  useEffect(() => {
    const handle = window.setTimeout(() => {
      setPage(1);
      setSearch(searchInput.trim());
    }, 350);
    return () => window.clearTimeout(handle);
  }, [searchInput]);

  const baseColumns: ColumnsType<Person> = useMemo(
    () => [
      {
        title: "Nome",
        dataIndex: "name",
        key: "name",
        width: 240,
        ellipsis: true,
        render: (name: string) => <strong>{name}</strong>,
      },
      {
        title: "Sexo",
        dataIndex: "sex",
        key: "sex",
        width: 100,
        render: (sex: string) => <Tag>{sexLabel(sex)}</Tag>,
      },
      {
        title: "Raça",
        dataIndex: "race",
        key: "race",
        width: 120,
        ellipsis: true,
      },
      {
        title: "Escolaridade",
        dataIndex: "education",
        key: "education",
        width: 160,
        ellipsis: true,
      },
      {
        title: "Ações",
        key: "actions",
        width: 148,
        align: "right",
        render: (_, record) => (
          <Space size={4}>
            <Tooltip title="Editar">
              <Button
                type="text"
                icon={<EditOutlined />}
                onClick={() => {
                  setEditing(record);
                  setDrawerOpen(true);
                }}
              />
            </Tooltip>
            <Tooltip title="Árvore genealógica">
              <Button
                type="text"
                icon={<ApartmentOutlined />}
                onClick={() => navigate(`/family-tree/${record.id}`)}
              />
            </Tooltip>
            <Tooltip title="Excluir">
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                onClick={() => {
                  Modal.confirm({
                    title: "Excluir pessoa?",
                    content: `Confirma a exclusão de "${record.name}"? Esta ação não pode ser desfeita.`,
                    okText: "Excluir",
                    okType: "danger",
                    cancelText: "Cancelar",
                    onOk: async () => {
                      try {
                        await deleteMutation.mutateAsync(record.id);
                        message.success("Pessoa excluída");
                      } catch (err) {
                        message.error(parseApiError(err).message);
                      }
                    },
                  });
                }}
              />
            </Tooltip>
          </Space>
        ),
      },
    ],
    [deleteMutation, navigate]
  );

  const { columns, scrollX } = useResizableColumns(baseColumns);

  return (
    <div className="census-table-page">
      <PageHeader
        title="Pessoas"
        subtitle="Cadastro e consulta de cidadãos do censo"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditing(null);
              setDrawerOpen(true);
            }}
          >
            Nova pessoa
          </Button>
        }
      />

      <Space style={{ marginBottom: 16 }} wrap>
        <Input.Search
          allowClear
          placeholder="Buscar por nome"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          style={{ width: 320 }}
        />
      </Space>

      <div className="census-table-page__table" ref={containerRef}>
        <Table<Person>
          rowKey="id"
          loading={isLoading && !data}
          components={resizableTableComponents}
          columns={columns}
          dataSource={data?.items ?? []}
          scroll={{ x: scrollX, y: scrollY }}
          pagination={{
            current: page,
            pageSize: 10,
            total: data?.totalItems ?? 0,
            showSizeChanger: false,
            onChange: setPage,
            showTotal: (total) => `${total} registro(s)`,
          }}
        />
      </div>

      <PersonFormDrawer
        open={drawerOpen}
        person={editing}
        onClose={() => {
          setDrawerOpen(false);
          setEditing(null);
        }}
      />
    </div>
  );
}
