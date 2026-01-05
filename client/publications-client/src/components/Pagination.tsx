type Props = {
  pageNumber: number;
  pageSize: number;
  total: number;
  onChange: (next: { pageNumber: number; pageSize: number }) => void;
};

export default function Pagination({ pageNumber, pageSize, total, onChange }: Props) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div style={{ display: "flex", gap: 12, alignItems: "center", marginTop: 12 }}>
      <button
        disabled={pageNumber <= 1}
        onClick={() => onChange({ pageNumber: pageNumber - 1, pageSize })}
      >
        ◀ Prev
      </button>
      <span>
        Page <strong>{pageNumber}</strong> / {totalPages} ({total} items)
      </span>
      <button
        disabled={pageNumber >= totalPages}
        onClick={() => onChange({ pageNumber: pageNumber + 1, pageSize })}
      >
        Next ▶
      </button>
      <select
        value={pageSize}
        onChange={(e) => onChange({ pageNumber: 1, pageSize: Number(e.target.value) })}
      >
        <option value={5}>5</option>
        <option value={10}>10</option>
        <option value={25}>25</option>
      </select>
    </div>
  );
}
