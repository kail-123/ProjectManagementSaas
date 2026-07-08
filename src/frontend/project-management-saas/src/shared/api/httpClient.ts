import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../../features/auth/model/authStore';
import type { AuthenticationResponse } from '../../features/auth/model/authTypes';
import { environment } from '../config/environment';

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let refreshSessionPromise: Promise<string | null> | null = null;

export const httpClient = axios.create({
  baseURL: environment.apiBaseUrl,
  timeout: environment.apiTimeoutMs,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json'
  }
});

httpClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined;

    if (!shouldRefresh(error, originalRequest)) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;
    refreshSessionPromise ??= refreshAccessToken();

    const accessToken = await refreshSessionPromise.finally(() => {
      refreshSessionPromise = null;
    });

    if (!accessToken) {
      return Promise.reject(error);
    }

    originalRequest.headers.Authorization = `Bearer ${accessToken}`;

    return httpClient(originalRequest);
  }
);

function shouldRefresh(error: AxiosError, request: RetriableRequestConfig | undefined): request is RetriableRequestConfig {
  const url = request?.url ?? '';

  return Boolean(
    request
      && error.response?.status === 401
      && !request._retry
      && !url.includes('/auth/token')
      && !url.includes('/auth/refresh')
  );
}

async function refreshAccessToken(): Promise<string | null> {
  try {
    const response = await axios.post<AuthenticationResponse>(
      `${environment.apiBaseUrl}/auth/refresh`,
      undefined,
      {
        timeout: environment.apiTimeoutMs,
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json'
        }
      }
    );

    useAuthStore.getState().setSession(response.data);

    return response.data.accessToken;
  } catch {
    useAuthStore.getState().clearSession();

    return null;
  }
}
