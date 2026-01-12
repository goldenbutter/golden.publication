import { useState } from "react";

type Props = {
  initialTitle?: string;
  initialIsbn?: string;
  initialDescription?: string;
  onSearch: (q: { title?: string; isbn?: string; description?: string }) => void;
};

export default function SearchBar({ initialTitle, initialIsbn, initialDescription, onSearch }: Props) {
  const [title, setTitle] = useState(initialTitle ?? "");
  const [isbn, setIsbn] = useState(initialIsbn ?? "");
  const [description, setDescription] = useState(initialDescription ?? "");

  return (
    <div style={{ display: "flex", gap: 12, marginBottom: 12 }}>
      <input
        placeholder="Search title..."
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        style={{ padding: 8, width: 260 }}
      />
      <input
        placeholder="Search ISBN..."
        value={isbn}
        onChange={(e) => setIsbn(e.target.value)}
        style={{ padding: 8, width: 180 }}
      />
      <input
        placeholder="Search description..."
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        style={{ padding: 8, width: 260 }}
      />

      <button onClick={() => onSearch({ title, isbn, description })} style={{ padding: "8px 14px" }}>
        Search
      </button>
      <button
        onClick={() => {
          setTitle("");
          setIsbn("");
          onSearch({ title: "", isbn: "", description: "" });
        }}
        style={{ padding: "8px 14px" }}
      >
        Reset
      </button>
    </div>
  );
}
