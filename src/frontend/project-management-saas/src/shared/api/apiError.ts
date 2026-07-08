import { AxiosError } from 'axios';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function getApiErrorMessage(error: unknown, fallback: string) {
  if (!(error instanceof AxiosError)) {
    return fallback;
  }

  const data = error.response?.data as ProblemDetails | undefined;
  if (!data || typeof data !== 'object') {
    return fallback;
  }

  const validationMessages = data.errors
    ? Object.values(data.errors).flat().filter(Boolean)
    : [];

  if (validationMessages.length > 0) {
    return validationMessages.slice(0, 3).join(' ');
  }

  if (data.detail?.trim()) {
    return data.detail;
  }

  if (data.title?.trim()) {
    return data.title;
  }

  return fallback;
}
