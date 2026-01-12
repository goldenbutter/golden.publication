import type { PublicationsListResponse, PublicationDetails } from "./types";

// Read API base from Vite env at build time; fallback for local dev.
const API_BASE =
  (import.meta.env?.VITE_API_BASE as string | undefined) ?? "http://localhost:5031";

function buildQuery(params: {
  title?: string;
  isbn?: string;
  description?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;  
  sortDir?: string; 
}) {
  const qs = new URLSearchParams();
  if (params.title) qs.set("title", params.title);
  if (params.isbn) qs.set("isbn", params.isbn);
  if (params.description) qs.set("description", params.description);
  qs.set("pageNumber", String(params.pageNumber ?? 1));
  qs.set("pageSize", String(params.pageSize ?? 10));
  if (params.sortBy) qs.set("sortBy", params.sortBy);
  if (params.sortDir) qs.set("sortDir", params.sortDir);
  return qs.toString();
}

export async function fetchPublications(params: {
  title?: string;
  isbn?: string;
  description?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: string;
}): Promise<PublicationsListResponse> {
  const query = buildQuery(params);
  const url = `${API_BASE}/publications?${query}`;
  const res = await fetch(url);

  if (!res.ok) {
    console.error("fetchPublications failed", { url, status: res.status, statusText: res.statusText });
    throw new Error(`Failed to load publications: ${res.status} ${res.statusText}`);
  }
  return res.json();
}

export async function fetchPublicationById(id: string): Promise<PublicationDetails> {
  const url = `${API_BASE}/publications/${id}`;
  const res = await fetch(url);

  if (!res.ok) {
    console.error("fetchPublicationById failed", { url, status: res.status, statusText: res.statusText });
    throw new Error(`Publication not found: ${id} (${res.status} ${res.statusText})`);
  }
  return res.json();
}
