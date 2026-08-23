import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  InputNumber,
  Row,
  Select,
  Space,
  Typography,
} from "antd";
import {
  Background,
  Controls,
  MiniMap,
  ReactFlow,
  Handle,
  Position,
  type Edge,
  type Node,
  type NodeProps,
} from "@xyflow/react";
import dagre from "dagre";
import "@xyflow/react/dist/style.css";
import { PageHeader } from "@/shared/ui/PageHeader";
import { DeferredSkeleton } from "@/shared/ui/DeferredSkeleton";
import { useFamilyTree, usePeople } from "@/shared/api/hooks";
import type { PersonFamilyTree } from "@/shared/api/types";
import { parseApiError } from "@/shared/api/errors";

function PersonNode({ data }: NodeProps) {
  return (
    <div className="person-node">
      <Handle type="target" position={Position.Top} />
      <div className="person-node__name">{String(data.label)}</div>
      <div className="person-node__meta">{String(data.subtitle)}</div>
      <Handle type="source" position={Position.Bottom} />
    </div>
  );
}

const nodeTypes = { person: PersonNode };

function layoutTree(tree: PersonFamilyTree): { nodes: Node[]; edges: Edge[] } {
  const g = new dagre.graphlib.Graph();
  g.setDefaultEdgeLabel(() => ({}));
  g.setGraph({ rankdir: "TB", nodesep: 48, ranksep: 72 });

  for (const node of tree.nodes) {
    g.setNode(node.id, { width: 200, height: 72 });
  }
  for (const rel of tree.relationships) {
    g.setEdge(rel.parentId, rel.childId);
  }
  dagre.layout(g);

  const nodes: Node[] = tree.nodes.map((node) => {
    const pos = g.node(node.id);
    return {
      id: node.id,
      type: "person",
      position: { x: (pos?.x ?? 0) - 100, y: (pos?.y ?? 0) - 36 },
      data: {
        label: node.name,
        subtitle: `ID ${node.id.slice(0, 8)}…`,
      },
    };
  });

  const edges: Edge[] = tree.relationships.map((rel, index) => ({
    id: `e-${rel.parentId}-${rel.childId}-${index}`,
    source: rel.parentId,
    target: rel.childId,
    animated: false,
    style: { stroke: "#2563eb" },
  }));

  return { nodes, edges };
}

export function FamilyTreePage() {
  const { personId: routePersonId } = useParams();
  const navigate = useNavigate();
  const [personId, setPersonId] = useState<string | undefined>(routePersonId);
  const [level, setLevel] = useState(2);
  const [search, setSearch] = useState("");

  const { data: people, isFetching: searching } = usePeople(1, search);
  const {
    data: tree,
    isLoading,
    isFetching,
    error,
    refetch,
    isFetched,
  } = useFamilyTree(personId, level);

  useEffect(() => {
    if (routePersonId) setPersonId(routePersonId);
  }, [routePersonId]);

  const graph = useMemo(() => {
    if (!tree?.nodes?.length) return { nodes: [] as Node[], edges: [] as Edge[] };
    return layoutTree(tree);
  }, [tree]);

  const options = useMemo(
    () =>
      (people?.items ?? []).map((p) => ({
        value: p.id,
        label: p.name,
      })),
    [people]
  );

  const load = (id?: string) => {
    const next = id ?? personId;
    if (!next) return;
    setPersonId(next);
    navigate(`/family-tree/${next}`, { replace: true });
  };

  const apiError = error ? parseApiError(error).message : null;

  return (
    <div className="census-family-tree-page">
      <PageHeader
        title="Árvore genealógica"
        subtitle="Busque uma pessoa e explore relações familiares interativas"
      />

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={7}>
          <Card title="Busca">
            <Space direction="vertical" style={{ width: "100%" }} size="middle">
              <div>
                <Typography.Text type="secondary">Pessoa</Typography.Text>
                <Select
                  showSearch
                  allowClear
                  filterOption={false}
                  loading={searching}
                  placeholder="Digite o nome"
                  style={{ width: "100%", marginTop: 8 }}
                  options={options}
                  value={personId}
                  onSearch={setSearch}
                  onChange={(value) => setPersonId(value)}
                />
              </div>
              <div>
                <Typography.Text type="secondary">Níveis da árvore</Typography.Text>
                <InputNumber
                  min={1}
                  max={8}
                  value={level}
                  onChange={(v) => setLevel(Math.min(8, Math.max(1, Number(v ?? 1))))}
                  style={{ width: "100%", marginTop: 8 }}
                />
                <Typography.Paragraph type="secondary" style={{ marginTop: 8, marginBottom: 0 }}>
                  1 = pais e filhos diretos · valores maiores incluem mais gerações
                </Typography.Paragraph>
              </div>
              <Button
                type="primary"
                block
                onClick={() => {
                  load();
                  void refetch();
                }}
                disabled={!personId}
                loading={isFetching}
              >
                Carregar árvore
              </Button>
              {tree ? (
                <Typography.Text type="secondary">
                  {tree.nodes.length} pessoa(s) · {tree.relationships.length} vínculo(s)
                </Typography.Text>
              ) : null}
            </Space>
          </Card>
        </Col>
        <Col xs={24} lg={17}>
          <Card className="census-family-tree-card" styles={{ body: { padding: 0 } }}>
            {!personId ? (
              <div className="family-tree-canvas__placeholder">
                <Empty description="Selecione uma pessoa para visualizar a árvore" />
              </div>
            ) : apiError ? (
              <div className="family-tree-canvas__placeholder" style={{ alignItems: "flex-start" }}>
                <Alert
                  type="error"
                  showIcon
                  message="Não foi possível carregar a árvore"
                  description={apiError}
                  action={
                    <Button size="small" onClick={() => refetch()}>
                      Tentar novamente
                    </Button>
                  }
                />
              </div>
            ) : isLoading && !tree ? (
              <div className="family-tree-canvas__placeholder">
                <DeferredSkeleton loading paragraph={{ rows: 8 }}>
                  <div />
                </DeferredSkeleton>
              </div>
            ) : isFetched && !graph.nodes.length ? (
              <div className="family-tree-canvas__placeholder">
                <Empty description="Pessoa não encontrada na árvore genealógica. Cadastros antigos podem precisar ser re-salvos para sincronizar." />
              </div>
            ) : (
              <div className="family-tree-canvas census-fade-in">
                <ReactFlow
                  nodes={graph.nodes}
                  edges={graph.edges}
                  nodeTypes={nodeTypes}
                  fitView
                  minZoom={0.3}
                  maxZoom={1.6}
                  proOptions={{ hideAttribution: true }}
                >
                  <Background gap={18} color="#dbeafe" />
                  <MiniMap pannable zoomable />
                  <Controls showInteractive={false} />
                </ReactFlow>
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
}
