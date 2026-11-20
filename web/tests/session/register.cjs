const fs = require('node:fs');
const ts = require('typescript');

function compile(module, filename) {
  const source = fs.readFileSync(filename, 'utf8');
  const result = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022, jsx: ts.JsxEmit.ReactJSX } });
  module._compile(result.outputText, filename);
}
require.extensions['.ts'] = compile;
require.extensions['.tsx'] = compile;
