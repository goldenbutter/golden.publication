import { useEffect } from "react";

import { BrowserRouter, Routes, Route } from "react-router-dom";
import ProtectedRoute from "./auth/ProtectedRoute";
import PublicOnlyRoute from "./auth/PublicOnlyRoute";
import { useAuth } from "./auth/useAuth";
import PublicationsListPage from "./pages/PublicationsListPage";
import PublicationDetailsPage from "./pages/PublicationDetailsPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

export default function App() {
  const { bootstrap } = useAuth();

  useEffect(() => {
    void bootstrap();
  }, [bootstrap]);
  return (
    <BrowserRouter basename="/app">
      <Routes>
        <Route path="/login" element={<PublicOnlyRoute><LoginPage /></PublicOnlyRoute>} />
        <Route path="/register" element={<PublicOnlyRoute><RegisterPage /></PublicOnlyRoute>} />
        <Route path="/" element={<ProtectedRoute><PublicationsListPage /></ProtectedRoute>} />
        <Route path="/publications/:id" element={<ProtectedRoute><PublicationDetailsPage /></ProtectedRoute>} />
      </Routes>
    </BrowserRouter>
  );
}
