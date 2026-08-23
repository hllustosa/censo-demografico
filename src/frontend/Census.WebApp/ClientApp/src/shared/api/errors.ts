import type { AxiosError } from "axios";
import type { ProblemDetails } from "./index";

export function parseApiError(error: unknown): ProblemDetails & {
  fieldErrors: Record<string, string[]>;
  message: string;
} {
  const axiosError = error as AxiosError<ProblemDetails>;
  const data = axiosError?.response?.data;
  const fieldErrors = data?.errors ?? {};
  const fieldMessages = Object.values(fieldErrors).flat();

  let message =
    fieldMessages[0] ||
    data?.detail ||
    data?.title ||
    "Ocorreu um erro inesperado.";

  if (axiosError?.response?.status === 403) {
    message = "Você não tem permissão para executar esta operação.";
  } else if (axiosError?.response?.status === 429) {
    message = "Muitas tentativas. Aguarde alguns instantes e tente novamente.";
  }

  return {
    ...data,
    fieldErrors,
    message,
    status: axiosError?.response?.status ?? data?.status,
  };
}

/** Map ASP.NET validation keys (PascalCase) to antd Form field names (camelCase / nested). */
export function mapFieldErrorsToForm(
  fieldErrors: Record<string, string[]>
): { name: (string | number)[]; errors: string[] }[] {
  return Object.entries(fieldErrors).map(([key, errors]) => {
    const parts = key
      .replace(/^\$\./, "")
      .split(/[.\[]/)
      .filter(Boolean)
      .map((p) => p.replace(/\]$/, ""));

    const normalized = parts.map((part) =>
      part.length > 0 ? part.charAt(0).toLowerCase() + part.slice(1) : part
    );

    return { name: normalized, errors };
  });
}
