
import { BrowserRouter, Routes, Route } from "react-router-dom";
import PublicationsListPage from "./pages/PublicationsListPage";
import PublicationDetailsPage from "./pages/PublicationDetailsPage";

export default function App() {
  return (
    <BrowserRouter basename="/app">
      <Routes>
        <Route path="/" element={<PublicationsListPage />} />
        <Route path="/publications/:id" element={<PublicationDetailsPage />} />
      </Routes>
    </BrowserRouter>
  );
}
