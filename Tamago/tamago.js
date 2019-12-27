const fs = require('fs');
const path = require('path');
const Tamago = require('./build/package/Tamago');
const prettier = require('prettier');
const glob = require('glob');

const args = require('yargs')
  .scriptName('tamago')
  .usage('$0 <command> [options]')
  .command('format <file>', 'Formats <file>', {})
  .command('compile <file>', 'Compiles <file>', {})
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

const tamagoFiles = (cwd) => {
  return glob.sync('**/*.tamago.js', {
    cwd: cwd,
    absolute: true
  });
}

const baseFiles = tamagoFiles(path.join(__dirname, '../library'));
const projectFiles = (cwd) => baseFiles.concat(tamagoFiles(cwd));

const prepare = (cwd) => {
  const runtime = require('./runtime');
  const Tamago = global.Tamago = new runtime.TamagoRuntime();
  for (const file of projectFiles(cwd)) {
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
    const target = args.file + '.js';
    const js = compileFile(args.file);
    write(target, js);
    console.log(args.file, '->', target);
    break;
  }

  case 'run': {
    const Tamago = prepare(process.cwd());
    const module = Tamago.import_module(args.id);
    const main = module.$project("main:");
    console.log(main(process.argv.slice(2)));
    break;
  }

  case 'test': {
    const Tamago = prepare(process.cwd());
    process.exit(Tamago.run_tests());
    break;
  }
  
  default:
    throw new Error(`Unknown command ${args._[0]}`);
}