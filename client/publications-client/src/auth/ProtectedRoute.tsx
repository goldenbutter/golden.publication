import { type ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./useAuth";

type Props = {
  children: ReactNode;
};

export default function ProtectedRoute({ children }: Props) {
  const { state } = useAuth();

  if (state.status === "unknown") {
    return <p style={{ padding: 16 }}>Loading session...</p>;
  }

  if (state.status !== "authenticated") {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
