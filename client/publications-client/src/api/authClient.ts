import type { AuthResponse, AuthUser, LoginPayload, RegisterPayload } from "../auth/types";

const API_BASE = (import.meta.env?.VITE_API_BASE as string | undefined) ?? "http://localhost:5031";

async function parseError(res: Response): Promise<string> {
  try {
    const data = await res.json();
    return data?.detail ?? data?.title ?? `Request failed with status ${res.status}`;
  } catch {
    return `Request failed with status ${res.status}`;
  }
}

export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/register`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!res.ok) throw new Error(await parseError(res));
  return res.json();
}

export async function login(payload: LoginPayload): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!res.ok) throw new Error(await parseError(res));
  return res.json();
}

export async function refresh(): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/refresh`, {
    method: "POST",
    credentials: "include",
  });
  if (!res.ok) throw new Error(await parseError(res));
  return res.json();
}

export async function logout(): Promise<void> {
  const res = await fetch(`${API_BASE}/auth/logout`, {
    method: "POST",
    credentials: "include",
  });
  if (!res.ok && res.status !== 204) throw new Error(await parseError(res));
}

export async function me(accessToken: string): Promise<AuthUser> {
  const res = await fetch(`${API_BASE}/auth/me`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
  if (!res.ok) throw new Error(await parseError(res));
  return res.json();
}
