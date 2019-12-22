const fs = require('fs');
const Tamago = require('./build/package/Tamago');
const prettier = require('prettier');
const glob = require('glob');

const args = require('yargs')
  .scriptName('tamago')
  .usage('$0 <command> [options]')
  .command('format <file>', 'Formats <file>', {})
  .command('compile <file>', 'Compiles <file>', {})
  .command('run <id>', 'Runs module <id>', {})
  .help()
  .version()
  .demandCommand(1)
  .argv;

const read = (f) => fs.readFileSync(f, 'utf8');

const compileFile = (file) => {
  const source = read(file);
  const ast = Tamago.parse(source);
  const js = Tamago.generate(ast);
  const pretty = prettier.format(js, { parser: 'babel' });
  return pretty;
};

const projectFiles = (cwd) => {
  return glob.sync('**/*.tamago', {
    cwd: cwd,
    absolute: true
  });
}

const prepare = (cwd) => {
  const runtime = require('./runtime');
  const Tamago = global.Tamago = new runtime.TamagoRuntime();
  require.extensions['.tamago'] = (Module, filename) => {
    const js = compileFile(filename);
    Module._compile(js, filename);
  }
  for (const file of projectFiles(process.cwd())) {
    require(file);
  }
  Tamago.initialise();
  return Tamago;
}

switch (args._[0]) {
  case 'format': {
    const source = read(args.file);
    const ast = Tamago.parse(source);
    const formatted = Tamago.prettyPrint(ast);
    console.log(formatted);
    break;
  }

  case 'compile': {
    console.log(compileFile(args.file));
    break;
  }

  case 'run': {
    const Tamago = prepare();
    const module = Tamago.import_module(args.id);
    const main = module.$project("main:");
    console.log(main(process.argv.slice(2)));
    break;
  }

  case 'test': {
    const Tamago = prepare();
    process.exit(Tamago.run_tests());
    break;
  }
  
  default:
    throw new Error(`Unknown command ${args._[0]}`);
}