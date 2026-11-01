import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// don-chan's 3D view, which the WebUI shows in an iframe at /don-embed/ (src/embed/main.tsx), and which the server's
// avatar renderer opens too. Game files come from the server's api/Assets at runtime; nothing is bundled from disk.
// TaikoWebUI.csproj runs this build when node_modules is installed (npm install); the output is committed.
export default defineConfig({
  plugins: [react()],
  base: './',
  publicDir: false,
  build: {
    outDir: '../wwwroot/don-embed',
    emptyOutDir: true,
    rollupOptions: {
      input: 'embed.html',
      output: {
        // Fixed names: the server registers the bundle's files at build time, so hashed names would need a
        // server rebuild and restart after every embed rebuild. ASP.NET's static assets add their own ETags.
        entryFileNames: 'assets/[name].js',
        chunkFileNames: 'assets/[name].js',
        assetFileNames: 'assets/[name][extname]',
        manualChunks(id) {
          const p = id.split('\\').join('/')
          if (p.includes('/node_modules/three/')) return 'three-core'
          if (p.includes('/node_modules/')) return 'vendor'
        },
      },
    },
  },
})
