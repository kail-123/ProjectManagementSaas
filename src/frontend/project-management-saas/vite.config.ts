import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [react()],
  cacheDir: 'node_modules/.vite',
  optimizeDeps: {
    include: [
      '@emotion/react',
      '@emotion/styled',
      '@mui/material',
      '@tanstack/react-query',
      'axios',
      'framer-motion',
      'lucide-react',
      'react',
      'react-dom',
      'react-router-dom',
      'recharts',
      'zustand'
    ]
  },
  server: {
    host: '0.0.0.0',
    port: Number(process.env.VITE_DEV_SERVER_PORT ?? 5173)
  },
  preview: {
    host: '0.0.0.0',
    port: Number(process.env.VITE_DEV_SERVER_PORT ?? 5173)
  }
});
