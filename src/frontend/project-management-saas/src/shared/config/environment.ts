interface Environment {
  apiBaseUrl: string;
  apiTimeoutMs: number;
  enableQueryDevtools: boolean;
}

const apiBaseUrl = readRequiredString('VITE_API_BASE_URL', import.meta.env.VITE_API_BASE_URL);

export const environment: Environment = Object.freeze({
  apiBaseUrl: trimTrailingSlash(apiBaseUrl),
  apiTimeoutMs: readNumber('VITE_API_TIMEOUT_MS', import.meta.env.VITE_API_TIMEOUT_MS, 30000),
  enableQueryDevtools: import.meta.env.DEV
});

function readRequiredString(key: string, value: string | undefined): string {
  if (!value?.trim()) {
    throw new Error(`Missing environment variable: ${key}`);
  }

  return value;
}

function readNumber(key: string, value: string | undefined, fallback: number): number {
  if (!value?.trim()) {
    return fallback;
  }

  const parsed = Number(value);
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`Invalid numeric environment variable: ${key}`);
  }

  return parsed;
}

function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/, '');
}
