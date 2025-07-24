// ClientApp/vite.config.js
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  base: '/', // 部署在根目錄
  build: {
    emptyOutDir: true
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5094' 
    }
  }
})