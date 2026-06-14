import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import {defineConfig, loadEnv} from 'vite';

export default defineConfig(({mode}) => {
  const env = loadEnv(mode, '.', '');
  return {
    plugins: [react(), tailwindcss()],
    define: {
      'process.env.GEMINI_API_KEY': JSON.stringify(env.GEMINI_API_KEY),
    },
    base: '/event-horizon/',
    build: {
      outDir: path.resolve(__dirname, '../wwwroot/event-horizon'),
      emptyOutDir: true,
      rollupOptions: {
        output: {
          entryFileNames: 'assets/event-horizon.js',
          chunkFileNames: 'assets/chunk-[name].js',
          assetFileNames: (assetInfo) => {
            const name = assetInfo.name ?? '';
            if (name.endsWith('.css')) return 'assets/event-horizon.css';
            return 'assets/[name][extname]';
          },
        },
      },
    },
    resolve: {
      alias: {
        '@': path.resolve(__dirname, '.'),
      },
    },
    server: {
      hmr: process.env.DISABLE_HMR !== 'true',
    },
  };
});
