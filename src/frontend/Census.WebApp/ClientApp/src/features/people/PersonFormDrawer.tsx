import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Drawer,
  Form,
  Input,
  Select,
  Space,
  Spin,
  message,
} from "antd";
import type { FormInstance, Rule } from "antd/es/form";
import {
  EDUCATION_OPTIONS,
  RACE_OPTIONS,
  SEX_OPTIONS,
} from "@/shared/lib/constants";
import type { CreatePersonInput, Person } from "@/shared/api/types";
import { mapFieldErrorsToForm, parseApiError } from "@/shared/api/errors";
import {
  useCreatePerson,
  usePeople,
  usePerson,
  useUpdatePerson,
} from "@/shared/api/hooks";

type Props = {
  open: boolean;
  person?: Person | null;
  onClose: () => void;
};

type PersonFormValues = {
  name: string;
  sex: string;
  race: string;
  education: string;
  fatherId?: string;
  motherId?: string;
  address: {
    addressDesc: string;
    complement?: string;
    burrow: string;
    city: string;
    state: string;
    zipCode: string;
  };
};

const EMPTY_FORM_VALUES: PersonFormValues = {
  name: "",
  sex: "M",
  race: "Não Informada",
  education: "Alfabetizado(a)",
  address: {
    addressDesc: "",
    complement: "",
    burrow: "",
    city: "",
    state: "",
    zipCode: "",
  },
};

function buildFormValues(person?: Person | null): PersonFormValues {
  if (!person) {
    return EMPTY_FORM_VALUES;
  }

  return {
    name: person.name ?? "",
    sex: person.sex ?? "M",
    race: person.race ?? "Não Informada",
    education: person.education ?? "Alfabetizado(a)",
    fatherId: person.fatherId || undefined,
    motherId: person.motherId || undefined,
    address: {
      addressDesc: person.address?.addressDesc ?? "",
      complement: person.address?.complement ?? "",
      burrow: person.address?.burrow ?? "",
      city: person.address?.city ?? "",
      state: person.address?.state ?? "",
      zipCode: person.address?.zipCode ?? "",
    },
  };
}

function parentsMustDiffer(otherField: "fatherId" | "motherId"): Rule {
  return ({ getFieldValue }) => ({
    validator(_, value) {
      const otherValue = getFieldValue(otherField);
      if (value && otherValue && value === otherValue) {
        return Promise.reject(
          new Error("Pai e mãe não podem ser a mesma pessoa.")
        );
      }
      return Promise.resolve();
    },
  });
}

function PersonSearchSelect({
  form,
  name,
  label,
  otherField,
  selectedId,
}: {
  form: FormInstance;
  name: "fatherId" | "motherId";
  label: string;
  otherField: "fatherId" | "motherId";
  selectedId?: string;
}) {
  const [search, setSearch] = useState("");
  const { data, isFetching } = usePeople(1, search);
  const { data: selectedPerson } = usePerson(selectedId);

  const options = useMemo(() => {
    const map = new Map<string, string>();
    for (const p of data?.items ?? []) {
      map.set(p.id, p.name);
    }
    if (selectedPerson) {
      map.set(selectedPerson.id, selectedPerson.name);
    }
    return Array.from(map.entries()).map(([value, label]) => ({
      value,
      label,
    }));
  }, [data, selectedPerson]);

  return (
    <Form.Item
      name={name}
      label={label}
      required={false}
      dependencies={[otherField]}
      normalize={(value) => value || undefined}
      rules={[parentsMustDiffer(otherField)]}
    >
      <Select
        allowClear
        showSearch
        filterOption={false}
        loading={isFetching}
        placeholder="Opcional — buscar por nome"
        options={options}
        onSearch={setSearch}
        onClear={() => form.setFieldValue(name, undefined)}
      />
    </Form.Item>
  );
}

export function PersonFormDrawer({ open, person, onClose }: Props) {
  const [form] = Form.useForm<PersonFormValues>();
  const createMutation = useCreatePerson();
  const updateMutation = useUpdatePerson();
  const isEdit = Boolean(person?.id);
  const formKey = isEdit ? person!.id : "new";

  const {
    data: fullPerson,
    isLoading: loadingPerson,
    isFetching: fetchingPerson,
  } = usePerson(open && isEdit ? person?.id : undefined);

  const formPerson = isEdit ? fullPerson ?? null : null;
  const fatherId = Form.useWatch("fatherId", form);
  const motherId = Form.useWatch("motherId", form);

  useEffect(() => {
    if (!open) return;

    if (isEdit) {
      if (!fullPerson) return;
      form.setFieldsValue(buildFormValues(fullPerson));
      return;
    }

    form.resetFields();
    form.setFieldsValue(EMPTY_FORM_VALUES);
  }, [open, isEdit, fullPerson, form]);

  const saving = createMutation.isPending || updateMutation.isPending;
  const showLoading = isEdit && (loadingPerson || fetchingPerson) && !fullPerson;

  const requiredFields: (string | string[])[] = [
    "name",
    "sex",
    "race",
    "education",
    ["address", "addressDesc"],
    ["address", "burrow"],
    ["address", "city"],
    ["address", "zipCode"],
    ["address", "state"],
    "fatherId",
    "motherId",
  ];

  const submit = async () => {
    try {
      await form.validateFields(requiredFields);
      const values = form.getFieldsValue(true) as PersonFormValues;
      const body: CreatePersonInput = {
        name: values.name,
        sex: values.sex as CreatePersonInput["sex"],
        race: values.race as CreatePersonInput["race"],
        education: values.education as CreatePersonInput["education"],
        address: {
          addressDesc: values.address.addressDesc,
          complement: values.address.complement ?? "",
          burrow: values.address.burrow,
          city: values.address.city,
          state: values.address.state.toUpperCase(),
          zipCode: values.address.zipCode,
        },
      };
      if (values.fatherId) body.fatherId = values.fatherId;
      if (values.motherId) body.motherId = values.motherId;

      if (isEdit && person) {
        await updateMutation.mutateAsync({ id: person.id, body });
        message.success("Pessoa atualizada com sucesso");
      } else {
        await createMutation.mutateAsync(body);
        message.success("Pessoa cadastrada com sucesso");
      }
      onClose();
    } catch (err) {
      if ((err as { errorFields?: unknown }).errorFields) return;
      const parsed = parseApiError(err);
      const fields = mapFieldErrorsToForm(parsed.fieldErrors);
      if (fields.length) {
        form.setFields(fields as Parameters<typeof form.setFields>[0]);
      }
      message.error(parsed.message);
    }
  };

  return (
    <Drawer
      title={isEdit ? "Editar pessoa" : "Nova pessoa"}
      open={open}
      onClose={onClose}
      width={560}
      destroyOnClose
      extra={
        <Space>
          <Button onClick={onClose}>Cancelar</Button>
          <Button
            type="primary"
            loading={saving}
            disabled={showLoading}
            onClick={submit}
          >
            Salvar
          </Button>
        </Space>
      }
    >
      <Spin spinning={showLoading}>
        <Form
          key={formKey}
          form={form}
          layout="vertical"
          initialValues={buildFormValues(formPerson)}
        >
          <Form.Item
            name="name"
            label="Nome completo"
            rules={[
              { required: true, message: "Informe o nome" },
              { max: 100, message: "Máximo de 100 caracteres" },
            ]}
          >
            <Input placeholder="Nome da pessoa" />
          </Form.Item>

          <Space style={{ display: "flex" }} size="middle" align="start">
            <Form.Item
              name="sex"
              label="Sexo"
              style={{ flex: 1 }}
              rules={[{ required: true, message: "Selecione o sexo" }]}
            >
              <Select options={[...SEX_OPTIONS]} />
            </Form.Item>
            <Form.Item
              name="race"
              label="Raça"
              style={{ flex: 1 }}
              rules={[{ required: true, message: "Selecione a raça" }]}
            >
              <Select
                options={RACE_OPTIONS.map((v) => ({ value: v, label: v }))}
              />
            </Form.Item>
          </Space>

          <Form.Item
            name="education"
            label="Escolaridade"
            rules={[{ required: true, message: "Selecione a escolaridade" }]}
          >
            <Select
              options={EDUCATION_OPTIONS.map((v) => ({ value: v, label: v }))}
            />
          </Form.Item>

          <Space style={{ display: "flex" }} size="middle" align="start">
            <div style={{ flex: 1 }}>
              <PersonSearchSelect
                key={`${formKey}-father`}
                form={form}
                name="fatherId"
                label="Pai"
                otherField="motherId"
                selectedId={fatherId || formPerson?.fatherId || undefined}
              />
            </div>
            <div style={{ flex: 1 }}>
              <PersonSearchSelect
                key={`${formKey}-mother`}
                form={form}
                name="motherId"
                label="Mãe"
                otherField="fatherId"
                selectedId={motherId || formPerson?.motherId || undefined}
              />
            </div>
          </Space>

          <Form.Item
            name={["address", "addressDesc"]}
            label="Endereço"
            rules={[{ required: true, message: "Informe o endereço" }]}
          >
            <Input />
          </Form.Item>
          <Form.Item name={["address", "complement"]} label="Complemento">
            <Input />
          </Form.Item>

          <Space style={{ display: "flex" }} size="middle" align="start">
            <Form.Item
              name={["address", "burrow"]}
              label="Bairro"
              style={{ flex: 1 }}
              rules={[{ required: true, message: "Informe o bairro" }]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name={["address", "city"]}
              label="Cidade"
              style={{ flex: 1 }}
              rules={[{ required: true, message: "Informe a cidade" }]}
            >
              <Input />
            </Form.Item>
          </Space>

          <Space style={{ display: "flex" }} size="middle" align="start">
            <Form.Item
              name={["address", "zipCode"]}
              label="CEP"
              style={{ flex: 1 }}
              rules={[
                { required: true, message: "Informe o CEP" },
                {
                  pattern: /^\d{5}-?\d{3}$/,
                  message: "CEP inválido (ex: 20000-000)",
                },
              ]}
            >
              <Input placeholder="20000-000" />
            </Form.Item>
            <Form.Item
              name={["address", "state"]}
              label="UF"
              style={{ width: 120 }}
              rules={[
                { required: true, message: "UF" },
                { len: 2, message: "2 letras" },
              ]}
            >
              <Input maxLength={2} style={{ textTransform: "uppercase" }} />
            </Form.Item>
          </Space>
        </Form>
      </Spin>
    </Drawer>
  );
}
