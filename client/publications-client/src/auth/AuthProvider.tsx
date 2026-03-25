import { useCallback, useMemo, useState, type ReactNode } from "react";
import * as authClient from "../api/authClient";
import { AuthContext } from "./AuthContext";
import { clearAccessToken, setAccessToken } from "./tokenManager";
import type { AuthState, LoginPayload, RegisterPayload } from "./types";

type Props = {
  children: ReactNode;
};

const initialState: AuthState = {
  status: "unknown",
  accessToken: null,
  user: null,
};

export default function AuthProvider({ children }: Props) {
  const [state, setState] = useState<AuthState>(initialState);

  const applyAuthenticatedState = useCallback((response: Awaited<ReturnType<typeof authClient.login>>) => {
    setAccessToken(response.access_token);
    setState({
      status: "authenticated",
      accessToken: response.access_token,
      user: response.user,
    });
  }, []);

  const bootstrap = useCallback(async () => {
    try {
      const response = await authClient.refresh();
      applyAuthenticatedState(response);
    } catch {
      clearAccessToken();
      setState({ status: "unauthenticated", accessToken: null, user: null });
    }
  }, [applyAuthenticatedState]);

  const login = useCallback(async (payload: LoginPayload) => {
    const response = await authClient.login(payload);
    applyAuthenticatedState(response);
  }, [applyAuthenticatedState]);

  const register = useCallback(async (payload: RegisterPayload) => {
    const response = await authClient.register(payload);
    applyAuthenticatedState(response);
  }, [applyAuthenticatedState]);

  const logout = useCallback(async () => {
    try {
      await authClient.logout();
    } finally {
      clearAccessToken();
      setState({ status: "unauthenticated", accessToken: null, user: null });
    }
  }, []);

  const value = useMemo(() => ({ state, bootstrap, login, register, logout }), [state, bootstrap, login, register, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
