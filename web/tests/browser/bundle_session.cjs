const fs = require('fs');
const path = require('path');
const esbuild = require('esbuild');

const sourceDir = path.resolve(__dirname, '../../src/session');
const names = ['ApiError', 'boundedSignal', 'sessionEpoch', 'fetchRequest', 'dispatchUnsafe', 'unsafeRequest', 'api', 'expectedUser'];
function readSource(name) {
  for (const extension of ['.js', '.ts']) {
    const file = path.join(sourceDir, `${name}${extension}`);
    if (fs.existsSync(file)) {
      return { source: fs.readFileSync(file, 'utf8'), loader: extension === '.ts' ? 'ts' : 'js' };
    }
  }
  throw new Error(`Session source not found: ${name}`);
}

const modules = names.map((name) => {
  const { source, loader } = readSource(name);
  const code = esbuild.transformSync(source, { loader, format: 'cjs', target: 'es2020' }).code;
  return `${JSON.stringify('./' + name)}: function(module, exports, require) {\n${code}\n}`;
});

process.stdout.write(`(() => {
  const modules = {${modules.join(',\n')}};
  const cache = {};
  function require(name) {
    if (!modules[name]) throw new Error('Unknown test module');
    if (!cache[name]) {
      const module = {exports: {}};
      cache[name] = module;
      modules[name](module, module.exports, require);
    }
    return cache[name].exports;
  }
  window.sessionTestClient = { ...require('./api'), ...require('./expectedUser') };
})();`);
