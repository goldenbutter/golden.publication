import { useState } from "react";

type Props = {
  initialTitle?: string;
  initialIsbn?: string;
  onSearch: (q: { title?: string; isbn?: string }) => void;
};

export default function SearchBar({ initialTitle, initialIsbn, onSearch }: Props) {
  const [title, setTitle] = useState(initialTitle ?? "");
  const [isbn, setIsbn] = useState(initialIsbn ?? "");

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
      <button onClick={() => onSearch({ title, isbn })} style={{ padding: "8px 14px" }}>
        Search
      </button>
      <button
        onClick={() => {
          setTitle("");
          setIsbn("");
          onSearch({ title: "", isbn: "" });
        }}
        style={{ padding: "8px 14px" }}
      >
        Reset
      </button>
    </div>
  );
}
