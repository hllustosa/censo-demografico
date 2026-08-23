import { create } from "zustand";
import {
  clearAuthState,
  getAuthState,
  getRefreshToken,
  saveAuthState,
  type StoredAuth,
} from "@/shared/api/client";
import { authApi } from "@/shared/api/endpoints";
import type { UserProfile } from "@/shared/api/types";
import type { CensusRole } from "@/shared/lib/constants";

type AuthState = {
  user: UserProfile | null;
  accessToken: string | null;
  hydrated: boolean;
  hydrate: () => void;
  setSession: (auth: StoredAuth) => void;
  login: (email: string, password: string) => Promise<UserProfile>;
  logout: () => Promise<void>;
  hasAnyRole: (roles: CensusRole[]) => boolean;
};

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  hydrated: false,

  hydrate: () => {
    const stored = getAuthState();
    if (stored) {
      set({
        user: stored.user as UserProfile,
        accessToken: stored.accessToken,
        hydrated: true,
      });
    } else {
      set({ user: null, accessToken: null, hydrated: true });
    }
  },

  setSession: (auth) => {
    saveAuthState(auth);
    set({
      user: auth.user as UserProfile,
      accessToken: auth.accessToken,
    });
  },

  login: async (email, password) => {
    const { data } = await authApi.login(email, password);
    const session: StoredAuth = {
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      expiresAt: data.expiresAt,
      user: data.user,
    };
    get().setSession(session);
    return data.user;
  },

  logout: async () => {
    const refreshToken = getRefreshToken();
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } catch {
      // ignore logout errors
    } finally {
      clearAuthState();
      set({ user: null, accessToken: null });
    }
  },

  hasAnyRole: (roles) => {
    const userRoles = get().user?.roles ?? [];
    return roles.some((role) => userRoles.includes(role));
  },
}));
