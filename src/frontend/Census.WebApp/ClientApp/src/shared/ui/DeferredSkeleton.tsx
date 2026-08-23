import { useEffect, useState, type ReactNode } from "react";
import { Skeleton } from "antd";

type Props = {
  loading: boolean;
  children: ReactNode;
  delayMs?: number;
  active?: boolean;
  paragraph?: boolean | { rows?: number };
};

export function DeferredSkeleton({
  loading,
  children,
  delayMs = 200,
  active = true,
  paragraph = true,
}: Props) {
  const [showSkeleton, setShowSkeleton] = useState(false);

  useEffect(() => {
    if (!loading) {
      setShowSkeleton(false);
      return;
    }

    const timer = window.setTimeout(() => setShowSkeleton(true), delayMs);
    return () => window.clearTimeout(timer);
  }, [loading, delayMs]);

  if (loading && showSkeleton) {
    return <Skeleton active={active} paragraph={paragraph} />;
  }

  if (loading) {
    return <>{children}</>;
  }

  return <div className="census-fade-in">{children}</div>;
}
