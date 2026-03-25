import { refresh } from "./authClient";
import { clearAccessToken, getAccessToken, setAccessToken } from "../auth/tokenManager";

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      try {
        const refreshed = await refresh();
        setAccessToken(refreshed.access_token);
        return refreshed.access_token;
      } catch {
        clearAccessToken();
        return null;
      } finally {
        refreshPromise = null;
      }
    })();
  }
  return refreshPromise;
}

type ApiFetchInit = RequestInit & {
  skipAuthRetry?: boolean;
};

export async function apiFetch(input: RequestInfo | URL, init?: ApiFetchInit): Promise<Response> {
  const { skipAuthRetry, ...requestInit } = init ?? {};
  const token = getAccessToken();
  const headers = new Headers(requestInit.headers);
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(input, {
    ...requestInit,
    headers,
  });
  if (response.status !== 401 || skipAuthRetry) {
    return response;
  }

  const refreshedToken = await refreshAccessToken();
  if (!refreshedToken) {
    return response;
  }

  const retryHeaders = new Headers(requestInit.headers);
  retryHeaders.set("Authorization", `Bearer ${refreshedToken}`);
  return apiFetch(input, {
    ...requestInit,
    headers: retryHeaders,
    skipAuthRetry: true,
  });
}
