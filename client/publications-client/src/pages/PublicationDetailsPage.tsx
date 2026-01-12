import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { fetchPublicationById } from "../api/client";
import type { PublicationDetails } from "../api/types";

export default function PublicationDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const [data, setData] = useState<PublicationDetails | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        if (id) {
          const res = await fetchPublicationById(id);
          if (mounted) setData(res);
        }
      } catch (e: any) {
        setError(e.message ?? "Failed to load");
      } finally {
        if (mounted) setLoading(false);
      }
    }
    load();
    return () => {
      mounted = false;
    };
  }, [id]);

  return (
    <div style={{ maxWidth: 900, margin: "0 auto", padding: 16 }}>
      <Link to="/">&larr; Back</Link>
      <h1>Publication Details</h1>

      {loading && <p>Loading…</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}
      {!loading && !error && data && (
        <>
          <div style={{ marginBottom: 16 }}>
            <div><strong>Title:</strong> {data.title}</div>
            <div><strong>Type:</strong> {data.publication_type}</div>
            <div><strong>ISBN:</strong> {data.isbn}</div>
            <div><strong>ID:</strong> {data.id}</div>
            <div><strong>Description:</strong> {data.description}</div>
          </div>

          <h3>Versions</h3>
          <table width="100%" cellPadding={8} style={{ borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f5f5f5" }}>
                <th align="left">Version</th>
                <th align="left">Language</th>
                <th align="left">Cover Title</th>
                <th align="left">Version Id</th>
                <th align="left">Publication Guid</th>
              </tr>
            </thead>
            <tbody>
              {data.versions.map(v => (
                <tr key={v.id} style={{ borderTop: "1px solid #eee" }}>
                  <td>{v.version}</td>
                  <td>{v.language}</td>
                  <td>{v.cover_title}</td>
                  <td>{v.id}</td>
                  <td>{v.publication_guid}</td>
                </tr>
              ))}
              {data.versions.length === 0 && (
                <tr>
                  <td colSpan={5} style={{ padding: 16, color: "#777" }}>
                    No versions
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
