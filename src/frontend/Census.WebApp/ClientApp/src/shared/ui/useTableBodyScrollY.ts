import { useEffect, useRef, useState } from "react";

/** Keeps Ant Design Table body scroll inside a flex parent instead of the page. */
export function useTableBodyScrollY(chromeOffset = 112) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [scrollY, setScrollY] = useState(360);

  useEffect(() => {
    const el = containerRef.current;
    if (!el || typeof ResizeObserver === "undefined") return;

    const update = () => {
      setScrollY(Math.max(200, el.clientHeight - chromeOffset));
    };

    update();
    const observer = new ResizeObserver(update);
    observer.observe(el);
    return () => observer.disconnect();
  }, [chromeOffset]);

  return { containerRef, scrollY };
}
