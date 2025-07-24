import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');

    return {
        plugins: [react(), tailwindcss()],
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            },
        },
        server: {
            port: parseInt(env.VITE_PORT),
            proxy: {
                '/api': {
                    target: 'https://localhost:7167',
                    changeOrigin: true,
                    secure: false,
                }
            }
        },
        build: {
            outDir: 'dist',
            rollupOptions: {
                input: './index.html'
            }
        }
    }
})
