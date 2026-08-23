import { useState } from "react";
import { Alert, Button, Card, Form, Input, Typography } from "antd";
import { LockOutlined, MailOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "@/features/auth/authStore";
import { parseApiError } from "@/shared/api/errors";
import { CensusLogo } from "@/shared/ui/CensusLogo";

type LoginForm = {
  email: string;
  password: string;
};

export function LoginPage() {
  const login = useAuthStore((s) => s.login);
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const onFinish = async (values: LoginForm) => {
    setLoading(true);
    setError(null);
    try {
      await login(values.email, values.password);
      if (hasAnyRole(["Analyst", "Admin"])) {
        navigate("/dashboard", { replace: true });
      } else {
        navigate("/people", { replace: true });
      }
    } catch (err) {
      setError(parseApiError(err).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-shell">
      <Card className="login-card" styles={{ body: { padding: 32 } }}>
        <div style={{ textAlign: "center", marginBottom: 28 }}>
          <div style={{ marginBottom: 12 }}>
            <CensusLogo size={72} color="#4d5eae" />
          </div>
          <Typography.Title level={3} style={{ marginBottom: 4 }}>
            Censo Demográfico
          </Typography.Title>
          <Typography.Text type="secondary">
            Entre com suas credenciais para continuar
          </Typography.Text>
        </div>

        {error ? (
          <Alert
            type="error"
            message={error}
            showIcon
            style={{ marginBottom: 16 }}
          />
        ) : null}

        <Form<LoginForm>
          layout="vertical"
          requiredMark={false}
          initialValues={{
            email: "admin@censo.local",
            password: "Admin@12345",
          }}
          onFinish={onFinish}
        >
          <Form.Item
            name="email"
            label="E-mail"
            rules={[
              { required: true, message: "Informe o e-mail" },
              { type: "email", message: "E-mail inválido" },
            ]}
          >
            <Input prefix={<MailOutlined />} placeholder="voce@empresa.com" size="large" />
          </Form.Item>
          <Form.Item
            name="password"
            label="Senha"
            rules={[{ required: true, message: "Informe a senha" }]}
          >
            <Input.Password prefix={<LockOutlined />} size="large" />
          </Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            Entrar
          </Button>
        </Form>
      </Card>
    </div>
  );
}
