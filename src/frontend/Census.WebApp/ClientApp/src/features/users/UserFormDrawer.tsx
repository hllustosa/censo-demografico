import { useEffect } from "react";
import { Button, Drawer, Form, Input, Select, Space, message } from "antd";
import type { CreateUserRequest } from "@/shared/api/types";
import type { CensusRole } from "@/shared/lib/constants";
import { mapFieldErrorsToForm, parseApiError } from "@/shared/api/errors";
import { useCreateUser } from "@/shared/api/hooks";

type Props = {
  open: boolean;
  onClose: () => void;
};

const ROLE_OPTIONS: { value: CensusRole; label: string }[] = [
  { value: "Registrar", label: "Registrar" },
  { value: "Analyst", label: "Analyst" },
  { value: "Admin", label: "Admin" },
];

export function UserFormDrawer({ open, onClose }: Props) {
  const [form] = Form.useForm<CreateUserRequest>();
  const createMutation = useCreateUser();

  useEffect(() => {
    if (open) {
      form.resetFields();
      form.setFieldsValue({ roles: ["Registrar"] });
    }
  }, [open, form]);

  const submit = async () => {
    try {
      const values = await form.validateFields();
      await createMutation.mutateAsync(values);
      message.success("Usuário criado");
      onClose();
    } catch (err) {
      if ((err as { errorFields?: unknown }).errorFields) return;
      const apiError = parseApiError(err);
      const fields = mapFieldErrorsToForm(apiError.fieldErrors);
      if (fields.length) {
        form.setFields(fields as Parameters<typeof form.setFields>[0]);
      }
      message.error(apiError.message);
    }
  };

  return (
    <Drawer
      title="Novo usuário"
      open={open}
      onClose={onClose}
      width={560}
      destroyOnClose
      extra={
        <Space>
          <Button onClick={onClose}>Cancelar</Button>
          <Button type="primary" loading={createMutation.isPending} onClick={submit}>
            Criar
          </Button>
        </Space>
      }
    >
      <Form form={form} layout="vertical" initialValues={{ roles: ["Registrar"] }}>
        <Form.Item
          name="fullName"
          label="Nome completo"
          rules={[{ required: true, message: "Informe o nome" }]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="email"
          label="E-mail"
          rules={[
            { required: true, message: "Informe o e-mail" },
            { type: "email", message: "E-mail inválido" },
          ]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="password"
          label="Senha"
          rules={[
            { required: true, message: "Informe a senha" },
            { min: 8, message: "Mínimo de 8 caracteres" },
          ]}
        >
          <Input.Password />
        </Form.Item>
        <Form.Item
          name="roles"
          label="Roles"
          rules={[{ required: true, message: "Selecione ao menos uma role" }]}
        >
          <Select mode="multiple" options={ROLE_OPTIONS} />
        </Form.Item>
      </Form>
    </Drawer>
  );
}
