import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
function getErrorMessage(error: unknown) {
  if (error instanceof Error) return error.message;
  return "Registration failed";
}

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await register({ username, password });
      navigate("/", { replace: true });
    } catch (err: unknown) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "70vh", width: "100%" }}>
      <div style={{ maxWidth: 420, width: "100%", padding: 32, textAlign: "center", border: "1px solid #eee", borderRadius: 12, boxShadow: "0 4px 12px rgba(0,0,0,0.08)", backgroundColor: "white" }}>
        <h1 style={{ color: "red", fontSize: "1.8rem", margin: "0 0 24px 0", whiteSpace: "nowrap" }}>🔥 GOLDEN PUBLICATIONS 🔥</h1>
        <h2 style={{ marginBottom: 24, color: "#333" }}>REGISTRATION FORM</h2>
        <form onSubmit={onSubmit} style={{ textAlign: "left" }}>
          <div style={{ marginBottom: 16 }}>
            <label htmlFor="username" style={{ display: "block", marginBottom: 4, fontWeight: "bold", color: "#555" }}>Username</label>
            <input id="username" value={username} onChange={(e) => setUsername(e.target.value)} minLength={3} required style={{ display: "block", width: "100%", padding: "10px", boxSizing: "border-box", border: "1px solid #ccc", borderRadius: 4 }} />
          </div>
          <div style={{ marginBottom: 20 }}>
            <label htmlFor="password" style={{ display: "block", marginBottom: 4, fontWeight: "bold", color: "#555" }}>Password</label>
            <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} minLength={4} required style={{ display: "block", width: "100%", padding: "10px", boxSizing: "border-box", border: "1px solid #ccc", borderRadius: 4 }} />
          </div>
          {error && <p style={{ color: "red", marginBottom: 16, fontSize: "0.9rem" }}>{error}</p>}
          <button type="submit" disabled={loading} style={{ width: "100%", padding: "12px", backgroundColor: "#28a745", color: "white", border: "none", borderRadius: 4, cursor: "pointer", fontWeight: "bold" }}>
            {loading ? "Creating account..." : "Create account"}
          </button>
        </form>
        <p style={{ marginTop: 20, color: "#666" }}>
          Already have an account? <Link to="/login" style={{ color: "#007bff", textDecoration: "none", fontWeight: "500" }}>Login here</Link>
        </p>
      </div>
    </div>
  );
}
