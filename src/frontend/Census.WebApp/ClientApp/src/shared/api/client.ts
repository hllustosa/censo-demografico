import axios from "axios";

const STORAGE_KEY = "census.auth";

export type StoredAuth = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    roles: string[];
  };
};

export function getAuthState(): StoredAuth | null {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredAuth;
  } catch {
    return null;
  }
}

export function saveAuthState(state: StoredAuth) {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

export function clearAuthState() {
  sessionStorage.removeItem(STORAGE_KEY);
}

export function getAccessToken() {
  return getAuthState()?.accessToken ?? null;
}

export function getRefreshToken() {
  return getAuthState()?.refreshToken ?? null;
}

export const api = axios.create({
  baseURL: "/",
  headers: { "Content-Type": "application/json" },
});

let refreshPromise: Promise<boolean> | null = null;
let onUnauthorized: (() => void) | null = null;

export function setUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler;
}

async function tryRefreshToken(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  try {
    const { data } = await axios.post("/auth/api/v1/auth/refresh", {
      refreshToken,
    });
    saveAuthState({
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      expiresAt: data.expiresAt,
      user: data.user,
    });
    return true;
  } catch {
    clearAuthState();
    return false;
  }
}

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      getRefreshToken()
    ) {
      originalRequest._retry = true;
      refreshPromise = refreshPromise ?? tryRefreshToken();
      const refreshed = await refreshPromise;
      refreshPromise = null;

      if (refreshed) {
        originalRequest.headers.Authorization = `Bearer ${getAccessToken()}`;
        return api(originalRequest);
      }
    }

    if (error.response?.status === 401) {
      clearAuthState();
      onUnauthorized?.();
    }

    return Promise.reject(error);
  }
);
