import { useCallback, useMemo, useState, type SyntheticEvent } from "react";
import type { ColumnsType, ColumnType } from "antd/es/table";
import type { ResizeCallbackData } from "react-resizable";
import { ResizableTitle } from "./ResizableTitle";

type WidthMap = Record<string, number>;

function columnKey<T>(column: ColumnType<T>, index: number): string {
  if (typeof column.key === "string" || typeof column.key === "number") {
    return String(column.key);
  }
  if (Array.isArray(column.dataIndex)) {
    return column.dataIndex.join(".");
  }
  if (column.dataIndex != null) {
    return String(column.dataIndex);
  }
  return `col-${index}`;
}

function initialWidth<T>(column: ColumnType<T>): number | undefined {
  if (typeof column.width === "number") return column.width;
  if (typeof column.width === "string") {
    const parsed = Number.parseInt(column.width, 10);
    return Number.isFinite(parsed) ? parsed : undefined;
  }
  return undefined;
}

export type ResizableColumnsResult<T> = {
  columns: ColumnsType<T>;
  /** Pass to Table `scroll.x` so resized columns can expand past the viewport. */
  scrollX: number;
};

/**
 * Makes Ant Design Table columns resizable by dragging the header edge.
 * Columns need a numeric `width` (and preferably a `key` / `dataIndex`) to resize.
 */
export function useResizableColumns<T extends object>(
  baseColumns: ColumnsType<T>
): ResizableColumnsResult<T> {
  const [widths, setWidths] = useState<WidthMap>(() => {
    const map: WidthMap = {};
    baseColumns.forEach((column, index) => {
      if ("children" in column && column.children) return;
      const width = initialWidth(column as ColumnType<T>);
      if (width != null) map[columnKey(column as ColumnType<T>, index)] = width;
    });
    return map;
  });

  const handleResize = useCallback(
    (key: string) =>
      (_: SyntheticEvent, { size }: ResizeCallbackData) => {
        setWidths((prev) => ({
          ...prev,
          [key]: Math.max(72, Math.round(size.width)),
        }));
      },
    []
  );

  return useMemo(() => {
    let scrollX = 0;
    const columns = baseColumns.map((column, index) => {
      if ("children" in column && column.children) return column;

      const col = column as ColumnType<T>;
      const key = columnKey(col, index);
      const width = widths[key] ?? initialWidth(col);
      if (width == null) return col;

      scrollX += width;
      return {
        ...col,
        width,
        onHeaderCell: () => ({
          width,
          onResize: handleResize(key),
        }),
      };
    });

    return { columns, scrollX: Math.max(scrollX, 600) };
  }, [baseColumns, widths, handleResize]);
}

export const resizableTableComponents = {
  header: { cell: ResizableTitle },
};
