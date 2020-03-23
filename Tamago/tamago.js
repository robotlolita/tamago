#!/usr/bin/env node
const fs = require('fs');
const path = require('path');
const Tamago = require('./build/package/Tamago');
const Runtime = require("../runtime");
const prettier = require('prettier');
const glob = require('glob');

const args = require('yargs')
  .scriptName('tamago')
  .usage('$0 <command> [options]')
  .command('compile-file <file>', 'Compiles a specific file', {})
  .command('compile', 'Compiles all files under directory', {})
  .command('run <id>', 'Runs module <id>', {})
  .command('test', 'Runs tests', {})
  .help()
  .version()
  .demandCommand(1)
  .argv;

const read = (f) => fs.readFileSync(f, 'utf8');

const write = (f, d) => fs.writeFileSync(f, d, 'utf8');

const compileFile = (file) => {
  const source = read(file);
  const ast = Tamago.parse(source);
  const js = Tamago.generate(ast);
  const pretty = prettier.format(js, { parser: 'babel' });
  return pretty;
};

const tamagoJsFiles = (cwd) => {
  return glob.sync('**/*.tamago.js', {
    cwd: cwd,
    absolute: true
  });
}

const tamagoFiles = (cwd) => {
  return glob.sync('**/*.tamago', {
    cwd: cwd,
    absolute: true
  });
}

const libraryRoot = path.join(__dirname, "../library");

const prepareStdlib = () => {
  const NativeRuntime = new Runtime.Tamago();
  NativeRuntime.use_natives();
  for (const m of tamagoJsFiles(libraryRoot)) {
    require(m)(NativeRuntime);
  }
  const modules = [];
  for (const m of NativeRuntime._namespaces.values()) {
    if (!/unsafe/.test(m._id)) {
      modules.push(m);
    }
  }
  return modules;
}

const prepare = (cwd) => {
  const Tamago = new Runtime.Tamago();
  const stdlib = prepareStdlib();
  for (const m of stdlib) {
    Tamago._namespaces.set(m._id, m);
  }
  
  if (path.resolve(cwd) !== path.resolve(libraryRoot)) {
    for (const file of tamagoJsFiles(cwd)) {
      require(file)(Tamago);
    }
  }

  Tamago.initialise();
  return Tamago;
}

switch (args._[0]) {
  case 'compile': {
    const files = tamagoFiles(process.cwd());
    for (const file of files) {
      console.log(`Compiling`, file);
      const target = file + '.js';
      const js = compileFile(file);
      write(target, js);
    }
    break;
  }

  case 'compile-file': {
    console.log(`Compiling`, file);
    const file = args.file;
    const target = file + '.js';
    const js = compileFile(file);
    write(target, js);
    break;
  }

  case 'run': {
    const Tamago = prepare(process.cwd());
    const ns = Tamago.use_namespace(args.id);
    const main = ns["@t:main:"];
    const result = main(process.argv.slice(2));
    if (Tamago.is_effect(result)) {
      Tamago.run_effects(result, () => {
        throw new Error(`Not handled`);
      }).then(result => {
        if (result != null) {
          console.log(Tamago.show(result));
        }
      }).catch(error => {
        console.log(error);
        process.exitCode = 1;
      })
    } else {
      console.log(Tamago.show(result));
    }
    break;
  }

  case 'test': {
    const Tamago = prepare(process.cwd());
    process.exit(Tamago.run_tests().length);
    break;
  }
  
  default:
    throw new Error(`Unknown command ${args._[0]}`);
}