import * as authClient from "../api/authClient";
import { setAccessToken } from "./tokenManager";
import type { AuthResponse, LoginPayload, RegisterPayload } from "./types";

export async function loginWithCredentials(payload: LoginPayload): Promise<AuthResponse> {
  const response = await authClient.login(payload);
  setAccessToken(response.access_token);
  return response;
}

export async function registerAndLogin(payload: RegisterPayload): Promise<AuthResponse> {
  const response = await authClient.register(payload);
  setAccessToken(response.access_token);
  return response;
}

export async function refreshSession(): Promise<AuthResponse> {
  const response = await authClient.refresh();
  setAccessToken(response.access_token);
  return response;
}
