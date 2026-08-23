import { Typography } from "antd";

type Props = {
  title: string;
  subtitle?: string;
  extra?: React.ReactNode;
};

export function PageHeader({ title, subtitle, extra }: Props) {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "flex-start",
        gap: 16,
        marginBottom: 20,
      }}
    >
      <div>
        <Typography.Title level={2} className="page-title" style={{ margin: 0 }}>
          {title}
        </Typography.Title>
        {subtitle ? (
          <p className="page-subtitle">{subtitle}</p>
        ) : null}
      </div>
      {extra}
    </div>
  );
}
