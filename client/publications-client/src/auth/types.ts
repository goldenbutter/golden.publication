export type AuthUser = {
  id: string;
  username: string;
  role: "admin" | "user";
};

export type AuthResponse = {
  access_token: string;
  expires_at: string;
  user: AuthUser;
};

export type AuthState = {
  status: "unknown" | "authenticated" | "unauthenticated";
  accessToken: string | null;
  user: AuthUser | null;
};

export type LoginPayload = {
  username: string;
  password: string;
};

export type RegisterPayload = {
  username: string;
  password: string;
};
