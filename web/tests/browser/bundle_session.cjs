const fs = require('fs');
const path = require('path');
const ts = require('typescript');

const sourceDir = path.resolve(__dirname, '../../src/session');
const names = ['ApiError', 'boundedSignal', 'sessionEpoch', 'fetchRequest', 'dispatchUnsafe', 'unsafeRequest', 'api', 'expectedUser'];
const modules = names.map((name) => {
  const source = fs.readFileSync(path.join(sourceDir, `${name}.ts`), 'utf8');
  const code = ts.transpileModule(source, {
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2020 },
  }).outputText;
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
