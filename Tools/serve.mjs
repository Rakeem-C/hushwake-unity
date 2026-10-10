import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../Build/Web');
const types = { '.html': 'text/html; charset=utf-8', '.js': 'application/javascript', '.wasm': 'application/wasm', '.data': 'application/octet-stream', '.json': 'application/json', '.css': 'text/css', '.png': 'image/png', '.ico': 'image/x-icon' };
http.createServer((request, response) => {
  let file;
  try {
    const url = new URL(request.url, 'http://127.0.0.1');
    file = path.resolve(root, '.' + decodeURIComponent(url.pathname === '/' ? '/index.html' : url.pathname));
    if (!file.startsWith(root + path.sep)) throw new Error('Outside build');
  } catch { response.writeHead(400); response.end('Invalid path'); return; }
  fs.stat(file, (error, stat) => {
    if (error || !stat.isFile()) { response.writeHead(404); response.end('Not found'); return; }
    const zipped=file.endsWith('.gz');
    response.writeHead(200, { 'Content-Type': types[path.extname(zipped?file.slice(0,-3):file)] || 'application/octet-stream', 'Content-Length': stat.size, 'Cache-Control': 'no-store',...(zipped?{'Content-Encoding':'gzip'}:{}) });
    fs.createReadStream(file).pipe(response);
  });
}).listen(43827, '127.0.0.1', () => process.stdout.write('Hushwake local Web build: http://127.0.0.1:43827\n'));
