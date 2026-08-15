"use client";

import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { API_BASE_URL } from "@/lib/config";

interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

interface AuthContextValue {
  accessToken: string | null;
  isLoading: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  // Attaches the current access token, retries once after a silent refresh
  // on 401. The refresh token itself is an httpOnly cookie this code never
  // reads — credentials: "include" is what carries it to Bimss.Api.
  fetchWithAuth: (path: string, init?: RequestInit) => Promise<Response>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const accessTokenRef = useRef<string | null>(null);

  const setToken = useCallback((token: string | null) => {
    accessTokenRef.current = token;
    setAccessToken(token);
  }, []);

  const refresh = useCallback(async (): Promise<string | null> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });

    if (!response.ok) {
      setToken(null);
      return null;
    }

    const body = (await response.json()) as LoginResponse;
    setToken(body.accessToken);
    return body.accessToken;
  }, [setToken]);

  useEffect(() => {
    // Attempt to restore a session from the refresh cookie on first load
    // (e.g. after a page refresh) rather than forcing a fresh login every
    // time. One-shot async initialization on mount has no clean
    // effect-free alternative here without pulling in a data-fetching
    // library, which is out of scope for this scaffold.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void refresh().finally(() => setIsLoading(false));
  }, [refresh]);

  const login = useCallback(
    async (userName: string, password: string) => {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password }),
      });

      if (!response.ok) {
        throw new Error("Invalid username or password.");
      }

      const body = (await response.json()) as LoginResponse;
      setToken(body.accessToken);
    },
    [setToken],
  );

  const logout = useCallback(async () => {
    await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: "POST",
      credentials: "include",
    });
    setToken(null);
  }, [setToken]);

  const fetchWithAuth = useCallback(
    async (path: string, init: RequestInit = {}): Promise<Response> => {
      const attempt = async (token: string | null) => {
        const headers = new Headers(init.headers);
        if (token) {
          headers.set("Authorization", `Bearer ${token}`);
        }

        return fetch(`${API_BASE_URL}${path}`, { ...init, headers, credentials: "include" });
      };

      let response = await attempt(accessTokenRef.current);

      if (response.status === 401) {
        const newToken = await refresh();
        response = await attempt(newToken);
      }

      return response;
    },
    [refresh],
  );

  return (
    <AuthContext.Provider value={{ accessToken, isLoading, login, logout, fetchWithAuth }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }

  return context;
}
