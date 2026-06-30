/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        // Nuevo estilo: acento índigo elegante + sidebar profundo
        sidebar: { DEFAULT: "#151b2e", hover: "#222a45", active: "#4f46e5" },
        primary: { DEFAULT: "#4f46e5", hover: "#4338ca", soft: "#eef2ff" },
        // Design system institucional (otras pantallas)
        navy: { DEFAULT: "#39394E", 700: "#2f2f40", 900: "#23232f" },
        brand: { red: "#E41D26", yellow: "#F8EC08" },
      },
      fontFamily: {
        heading: ['"Playfair Display"', "serif"],
        sans: ["Inter", "system-ui", "sans-serif"],
      },
      boxShadow: {
        card: "0 1px 2px 0 rgb(16 24 40 / 0.04), 0 1px 3px 0 rgb(16 24 40 / 0.05)",
        soft: "0 10px 30px -12px rgb(79 70 229 / 0.18)",
        glow: "0 8px 24px -8px rgb(79 70 229 / 0.45)",
      },
      borderRadius: {
        "2xl": "1rem",
        "3xl": "1.5rem",
      },
    },
  },
  plugins: [],
};
