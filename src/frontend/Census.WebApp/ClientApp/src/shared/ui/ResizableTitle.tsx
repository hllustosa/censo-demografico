import type { ReactNode, SyntheticEvent, ThHTMLAttributes } from "react";
import { Resizable, type ResizeCallbackData } from "react-resizable";
import "react-resizable/css/styles.css";

type ResizableTitleProps = {
  onResize?: (e: SyntheticEvent, data: ResizeCallbackData) => void;
  width?: number | string;
  children?: ReactNode;
} & ThHTMLAttributes<HTMLTableCellElement>;

/** Header cell with drag handle — Ant Design Table resizable-column pattern. */
export function ResizableTitle({
  onResize,
  width,
  children,
  ...rest
}: ResizableTitleProps) {
  if (width == null || !onResize) {
    return <th {...rest}>{children}</th>;
  }

  const numericWidth =
    typeof width === "number" ? width : Number.parseInt(String(width), 10);
  if (!Number.isFinite(numericWidth)) {
    return <th {...rest}>{children}</th>;
  }

  return (
    <Resizable
      width={numericWidth}
      height={0}
      axis="x"
      minConstraints={[72, 0]}
      onResize={onResize}
      draggableOpts={{ enableUserSelectHack: false }}
      handle={
        <span
          className="census-resize-handle"
          onClick={(e) => e.stopPropagation()}
        />
      }
    >
      <th {...rest}>{children}</th>
    </Resizable>
  );
}
