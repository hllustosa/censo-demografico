import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    port: 3000,
    proxy: {
      "/auth": "http://localhost:8080",
      "/person": "http://localhost:8080",
      "/stats": "http://localhost:8080",
      "/family": "http://localhost:8080",
    },
  },
  build: {
    outDir: "dist",
    sourcemap: true,
  },
});
