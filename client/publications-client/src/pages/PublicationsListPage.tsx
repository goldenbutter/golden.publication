import { useEffect, useState } from "react";
import { fetchPublications } from "../api/client";
import type { PublicationListItem, PublicationsListResponse } from "../api/types";
import SearchBar from "../components/SearchBar";
import SortControls from "../components/SortControls";
import Pagination from "../components/Pagination";
import { Link } from "react-router-dom";

export default function PublicationsListPage() {
  const [items, setItems] = useState<PublicationListItem[]>([]);
  const [total, setTotal] = useState(0);

  const [title, setTitle] = useState<string>("");
  const [isbn, setIsbn] = useState<string>("");
  const [description, setDescription] = useState<string>("");

  const [sortBy, setSortBy] = useState<string>("title");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");

  const [pageNumber, setPageNumber] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);

  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const data: PublicationsListResponse = await fetchPublications({
        title, isbn, description, pageNumber, pageSize, sortBy, sortDir,
      });
      setItems(data.items);
      setTotal(data.total);
    } catch (e: any) {
      setError(e.message ?? "Failed to load");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [title, isbn, description, pageNumber, pageSize, sortBy, sortDir]);

  return (
    <div style={{ maxWidth: 1000, margin: "0 auto", padding: 16 }}>
	  <h1 style={{ color: "red" }}>🔥 GOLDEN PUBLICATIONS 🔥</h1>
      <SearchBar
        initialTitle={title}
        initialIsbn={isbn}
        initialDescription={description}
        onSearch={(q) => {
          setTitle(q.title ?? "");
          setIsbn(q.isbn ?? "");
          setDescription(q.description ?? "");
          setPageNumber(1);
        }}
      />

      <SortControls
        sortBy={sortBy}
        sortDir={sortDir}
        onChange={(v) => {
          setSortBy(v.sortBy);
          setSortDir(v.sortDir);
          setPageNumber(1);
        }}
      />

      {loading && <p>Loading…</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {!loading && !error && (
        <>
          <table width="100%" cellPadding={8} style={{ borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f5f5f5" }}>
                <th align="left">Title</th>
                <th align="left">Type</th>
                <th align="left">ISBN</th>
                <th align="left">Description</th>
                <th align="left">Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((p) => (
                <tr key={p.id} style={{ borderTop: "1px solid #eee" }}>
                  <td>{p.title}</td>
                  <td>{p.publication_type}</td>
                  <td>{p.isbn}</td>
                  <td>{p.description}</td>
                  <td>
                    <Link to={`/publications/${p.id}`}>View details</Link>
                  </td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr>
                  <td colSpan={4} style={{ padding: 16, color: "#777" }}>
                    No results
                  </td>
                </tr>
              )}
            </tbody>
          </table>

          <Pagination
            pageNumber={pageNumber}
            pageSize={pageSize}
            total={total}
            onChange={({ pageNumber, pageSize }) => {
              setPageNumber(pageNumber);
              setPageSize(pageSize);
            }}
          />
        </>
      )}
    </div>
  );
}
