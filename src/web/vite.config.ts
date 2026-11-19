import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vitest/config'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    // IPv4 loopback: Node 22 resolves "localhost" to ::1 only, and VS Code port forwarding connects to 127.0.0.1.
    host: '127.0.0.1',
    port: 5173,
    strictPort: true,
    // make dev / the Full stack debug compound: the host runs on its https launch profile.
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        // Development certificate, not trusted inside the container. Development only.
        secure: false,
      },
    },
  },
  build: {
    // The .NET host serves the SPA; dotnet build runs this build (S0-7, docs/environment.md §4).
    outDir: '../Crm.Host/wwwroot',
    emptyOutDir: true,
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    restoreMocks: true,
  },
})
