const fs = require('node:fs');
const path = require('node:path');
const esbuild = require('esbuild');

const sourceDir = path.resolve(__dirname, '../../src') + path.sep;

function loaderFor(filename) {
  if (filename.endsWith('.tsx')) return 'tsx';
  if (filename.endsWith('.jsx')) return 'jsx';
  if (filename.endsWith('.js')) return 'js';
  return 'ts';
}

function compile(module, filename) {
  const source = fs.readFileSync(filename, 'utf8');
  const result = esbuild.transformSync(source, {
    loader: loaderFor(filename),
    format: 'cjs',
    target: 'es2022',
    jsx: 'automatic',
  });
  module._compile(result.code, filename);
}

require.extensions['.ts'] = compile;
require.extensions['.tsx'] = compile;
require.extensions['.jsx'] = compile;

// package.json uses "type": "module", so Node would load .js files as ES modules.
// Compile only the app's own source files here; leave everything else to Node.
const loadJavaScript = require.extensions['.js'];
require.extensions['.js'] = function (module, filename) {
  if (filename.startsWith(sourceDir)) return compile(module, filename);
  return loadJavaScript(module, filename);
};