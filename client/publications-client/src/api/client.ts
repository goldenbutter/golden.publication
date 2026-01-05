import type { PublicationsListResponse, PublicationDetails } from "./types";

// Use your API base URL. If using HTTPS (7005), change accordingly.
const API_BASE = "http://localhost:5031";

export async function fetchPublications(params: {
  title?: string;
  isbn?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;  // e.g. "title,publication_type"
  sortDir?: string; // "asc" | "desc"
}): Promise<PublicationsListResponse> {
  const qs = new URLSearchParams();

  if (params.title) qs.set("title", params.title);
  if (params.isbn) qs.set("isbn", params.isbn);
  qs.set("pageNumber", String(params.pageNumber ?? 1));
  qs.set("pageSize", String(params.pageSize ?? 10));
  if (params.sortBy) qs.set("sortBy", params.sortBy);
  if (params.sortDir) qs.set("sortDir", params.sortDir);

  const res = await fetch(`${API_BASE}/publications?${qs.toString()}`);
  if (!res.ok) {
    throw new Error(`Failed to load publications: ${res.status}`);
  }
  return res.json();
}

export async function fetchPublicationById(id: string): Promise<PublicationDetails> {
  const res = await fetch(`${API_BASE}/publications/${id}`);
  if (!res.ok) {
    throw new Error(`Publication not found: ${id}`);
  }
  return res.json();
}
