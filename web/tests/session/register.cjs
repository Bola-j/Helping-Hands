const fs = require('node:fs');
const esbuild = require('esbuild');

function compile(module, filename) {
  const source = fs.readFileSync(filename, 'utf8');
  const loader = filename.endsWith('.tsx') ? 'tsx' : 'ts';
  const result = esbuild.transformSync(source, { loader, format: 'cjs', target: 'es2022', jsx: 'automatic' });
  module._compile(result.code, filename);
}
require.extensions['.ts'] = compile;
require.extensions['.tsx'] = compile;
