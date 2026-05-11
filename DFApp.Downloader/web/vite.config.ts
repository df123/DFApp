import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 9551,
    proxy: {
      '/api': {
        target: 'http://localhost:9550',
        changeOrigin: true,
      },
    },
  },
})
