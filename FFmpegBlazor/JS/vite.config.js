// vite.config.js
import { resolve } from 'path';
import { defineConfig } from 'vite';
import { transform } from 'esbuild';

export default defineConfig({
    build: {
        outDir: '../wwwroot/js',
        lib: {
            // Could also be a dictionary or array of multiple entry points
            entry: resolve(__dirname, 'main.js'),
            formats: ['es', 'esm'],
            name: 'index',
            fileName: (format) => ({
                es: `index.js`,
                esm: `index.min.js`,
            })[format],
        },
    },
    plugins: [minifyEs()],
    optimizeDeps: {
        exclude: ['@ffmpeg/ffmpeg', '@ffmpeg/util'],
    },
})

function minifyEs() {
    return {
        name: 'minifyEs',
        renderChunk: {
            order: 'post',
            async handler(code, chunk, outputOptions) {
                if (outputOptions.format === 'es' && chunk.fileName.endsWith('.min.js')) {
                    return await transform(code, { minify: true });
                }
                return code;
            },
        }
    };
}