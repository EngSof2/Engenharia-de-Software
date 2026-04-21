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
    // Served by ASP.NET under /event-horizon/
    base: '/event-horizon/',
    build: {
      // __dirname is .template/event-horizon/event-horizon
      // We want repoRoot/ES2/wwwroot/event-horizon
      outDir: path.resolve(__dirname, '../../../ES2/wwwroot/event-horizon'),
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
      // HMR is disabled in AI Studio via DISABLE_HMR env var.
      // Do not modifyâfile watching is disabled to prevent flickering during agent edits.
      hmr: process.env.DISABLE_HMR !== 'true',
    },
  };
});
