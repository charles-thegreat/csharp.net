import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // In development the browser calls /api on Vite's own port and Vite forwards it to
    // ASP.NET Core, so the API needs no CORS policy. In production you'd serve both from
    // the same origin (or behind one reverse proxy) for the same effect.
    proxy: {
      '/api': 'http://localhost:5031',
    },
  },
})
