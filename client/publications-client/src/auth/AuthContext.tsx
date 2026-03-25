import { createContext } from "react";
import type { AuthState, LoginPayload, RegisterPayload } from "./types";

export type AuthContextValue = {
  state: AuthState;
  bootstrap: () => Promise<void>;
  login: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
