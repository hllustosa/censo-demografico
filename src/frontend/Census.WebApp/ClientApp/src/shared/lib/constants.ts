export const SEX_OPTIONS = [
  { value: "M", label: "Masculino" },
  { value: "F", label: "Feminino" },
  { value: "I", label: "Indefinido" },
] as const;

export const RACE_OPTIONS = [
  "Branco(a)",
  "Pardo(a)",
  "Negro(a)",
  "Amarelo(a)",
  "Indígena",
  "Não Informada",
] as const;

export const EDUCATION_OPTIONS = [
  "Analfabeto(a)",
  "Alfabetizado(a)",
  "Ensino Fundamental",
  "Ensino Médio",
  "Ensino Superior",
  "Pós-Graduação",
] as const;

export type Sex = (typeof SEX_OPTIONS)[number]["value"];
export type Race = (typeof RACE_OPTIONS)[number];
export type Education = (typeof EDUCATION_OPTIONS)[number];

export type CensusRole = "Registrar" | "Analyst" | "Admin";

export const ROLES = {
  Registrar: "Registrar",
  Analyst: "Analyst",
  Admin: "Admin",
} as const;

export function sexLabel(sex?: string) {
  return SEX_OPTIONS.find((o) => o.value === sex)?.label ?? sex ?? "—";
}
