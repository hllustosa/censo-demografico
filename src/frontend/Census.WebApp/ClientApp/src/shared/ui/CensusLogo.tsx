type CensusLogoProps = {
  size?: number;
  /** Default white — for dark surfaces. Use brand blue on light backgrounds. */
  color?: string;
  className?: string;
  title?: string;
};

/** Geometric mark in the style of the Brazilian Census (IBGE) logo. */
export function CensusLogo({
  size = 36,
  color = "#ffffff",
  className,
  title = "Censo Demográfico",
}: CensusLogoProps) {
  const height = size * (110 / 160);

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 160 110"
      width={size}
      height={height}
      fill="none"
      className={className}
      role="img"
      aria-label={title}
      style={{ flexShrink: 0 }}
    >
      <title>{title}</title>
      <g fill={color}>
        <g transform="translate(80 55) skewX(-16) translate(-50 -50)">
          <path d="M10 0h37.5v32.18A18 18 0 0 0 32.18 47.5H0V10C0 4.48 4.48 0 10 0z" />
          <path d="M52.5 0H90c5.52 0 10 4.48 10 10v37.5H67.82A18 18 0 0 1 52.5 32.18V0z" />
          <path d="M0 52.5h32.18A18 18 0 0 1 47.5 67.82V100H10c-5.52 0-10-4.48-10-10V52.5z" />
          <path d="M67.82 52.5H100V90c0 5.52-4.48 10-10 10H52.5V67.82A18 18 0 0 0 67.82 52.5z" />
        </g>
        <circle cx="80" cy="55" r="14.5" />
      </g>
    </svg>
  );
}
