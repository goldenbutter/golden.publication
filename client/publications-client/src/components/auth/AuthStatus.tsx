import { useAuth } from "../../auth/useAuth";

export default function AuthStatus() {
  const { state, logout } = useAuth();

  if (state.status !== "authenticated" || !state.user) {
    return null;
  }

  return (
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
      <small>
        Logged in as <strong>{state.user.username}</strong> ({state.user.role})
      </small>
      <button type="button" onClick={() => logout()}>
        Logout
      </button>
    </div>
  );
}
