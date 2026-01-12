type Props = {
  sortBy: string;
  sortDir: "asc" | "desc";
  onChange: (next: { sortBy: string; sortDir: "asc" | "desc" }) => void;
};

export default function SortControls({ sortBy, sortDir, onChange }: Props) {
  return (
    <div style={{ display: "flex", gap: 12, marginBottom: 12 }}>
      <select
        value={sortBy}
        onChange={(e) => onChange({ sortBy: e.target.value, sortDir })}
        style={{ padding: 8 }}
      >
        <option value="">No sort</option>
        <option value="title">Title</option>
        <option value="publication_type">Publication Type</option>
        <option value="isbn">ISBN</option>
        <option value="description">Description</option>
        <option value="title,publication_type,description">Title, Publication Type, Description</option>
      </select>
      <select
        value={sortDir}
        onChange={(e) =>
          onChange({ sortBy, sortDir: e.target.value as "asc" | "desc" })
        }
        style={{ padding: 8 }}
      >
        <option value="asc">Asc</option>
        <option value="desc">Desc</option>
      </select>
    </div>
  );
}
