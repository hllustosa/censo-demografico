import { useMemo, useState } from "react";
import { Card, Col, Empty, Form, Input, Row, Select, Space, Statistic } from "antd";
import { Column, Bar } from "@ant-design/charts";
import { PageHeader } from "@/shared/ui/PageHeader";
import { DeferredSkeleton } from "@/shared/ui/DeferredSkeleton";
import {
  useCities,
  useCityCounter,
  usePersonCategoryStats,
} from "@/shared/api/hooks";
import { useStatsNotifications } from "@/shared/api/useStatsNotifications";
import {
  EDUCATION_OPTIONS,
  RACE_OPTIONS,
  SEX_OPTIONS,
  sexLabel,
} from "@/shared/lib/constants";

type Filters = {
  name?: string;
  sex?: string;
  race?: string;
  education?: string;
};

function aggregateBy(
  rows: { sex: string; race: string; schoolLevel: string; count: number }[],
  key: "sex" | "race" | "schoolLevel"
) {
  const map = new Map<string, number>();
  for (const row of rows) {
    const label =
      key === "sex" ? sexLabel(row.sex) : (row[key] || "Não informado");
    map.set(label, (map.get(label) ?? 0) + Number(row.count ?? 0));
  }
  return [...map.entries()].map(([category, value]) => ({ category, value }));
}

export function DashboardPage() {
  useStatsNotifications(true);
  const [filters, setFilters] = useState<Filters>({});
  const [city, setCity] = useState<string | undefined>();

  const { data: stats, isLoading } = usePersonCategoryStats(filters);
  const { data: cities } = useCities();
  const { data: cityCounter, isLoading: cityLoading } = useCityCounter(city);

  const total = useMemo(
    () => (stats ?? []).reduce((sum, row) => sum + Number(row.count ?? 0), 0),
    [stats]
  );

  const bySex = useMemo(() => aggregateBy(stats ?? [], "sex"), [stats]);
  const byRace = useMemo(() => aggregateBy(stats ?? [], "race"), [stats]);
  const byEducation = useMemo(
    () => aggregateBy(stats ?? [], "schoolLevel"),
    [stats]
  );

  const cityBars = useMemo(() => {
    const counters = cityCounter?.personNameCounters ?? {};
    return Object.values(counters)
      .map((item) => ({
        category: item.name,
        value: Number(item.count ?? 0),
      }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 12);
  }, [cityCounter]);

  const columnConfig = {
    xField: "category",
    yField: "value",
    color: "#2563eb",
    columnStyle: { radiusTopLeft: 6, radiusTopRight: 6 },
    label: false as const,
    height: 280,
    autoFit: true,
  };

  return (
    <div>
      <PageHeader
        title="Dashboard"
        subtitle="Indicadores demográficos atualizados em tempo real"
      />

      <Card bordered={false} className="census-filters-card">
        <Form
          layout="inline"
          onValuesChange={(_, all) => setFilters(all)}
          style={{ rowGap: 12, justifyContent: "center" }}
        >
          <Form.Item name="name" label="Nome">
            <Input allowClear placeholder="Filtrar por nome" style={{ width: 180 }} />
          </Form.Item>
          <Form.Item name="sex" label="Sexo">
            <Select
              allowClear
              style={{ width: 140 }}
              options={[...SEX_OPTIONS]}
              placeholder="Todos"
            />
          </Form.Item>
          <Form.Item name="race" label="Raça">
            <Select
              allowClear
              style={{ width: 160 }}
              options={RACE_OPTIONS.map((v) => ({ value: v, label: v }))}
              placeholder="Todas"
            />
          </Form.Item>
          <Form.Item name="education" label="Escolaridade">
            <Select
              allowClear
              style={{ width: 180 }}
              options={EDUCATION_OPTIONS.map((v) => ({ value: v, label: v }))}
              placeholder="Todas"
            />
          </Form.Item>
        </Form>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} md={6}>
          <Card title="Total de pessoas" className="census-dashboard-stat-card">
            <DeferredSkeleton loading={isLoading && !stats} paragraph={false}>
              <Statistic value={total} valueStyle={{ fontSize: 54, fontWeight: 700 }} />
            </DeferredSkeleton>
          </Card>
        </Col>
        <Col xs={24} md={18}>
          <Card title="População por sexo">
            <DeferredSkeleton loading={isLoading && !stats}>
              {bySex.length ? (
                <Column data={bySex} {...columnConfig} />
              ) : (
                <Empty description="Sem dados" />
              )}
            </DeferredSkeleton>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="População por raça">
            <DeferredSkeleton loading={isLoading && !stats}>
              {byRace.length ? (
                <Column data={byRace} {...columnConfig} />
              ) : (
                <Empty description="Sem dados" />
              )}
            </DeferredSkeleton>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="População por escolaridade">
            <DeferredSkeleton loading={isLoading && !stats}>
              {byEducation.length ? (
                <Column data={byEducation} {...columnConfig} />
              ) : (
                <Empty description="Sem dados" />
              )}
            </DeferredSkeleton>
          </Card>
        </Col>
        <Col span={24}>
          <Card
            title="Nomes por cidade"
            extra={
              <Space>
                <Select
                  showSearch
                  allowClear
                  placeholder="Selecione a cidade"
                  style={{ width: 240 }}
                  options={(cities ?? []).map((c) => ({ value: c, label: c }))}
                  value={city}
                  onChange={setCity}
                />
              </Space>
            }
          >
            {!city ? (
              <Empty description="Selecione uma cidade para visualizar" />
            ) : (
              <DeferredSkeleton loading={cityLoading && !cityCounter}>
                {cityBars.length ? (
                  <Bar
                    data={cityBars}
                    xField="value"
                    yField="category"
                    color="#2563eb"
                    height={360}
                    autoFit
                    barStyle={{ radius: [0, 6, 6, 0] }}
                  />
                ) : (
                  <Empty description="Sem contagens para esta cidade" />
                )}
              </DeferredSkeleton>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
}
