const fs = require('fs');
const Tamago = require('./build/package/Tamago');
const prettier = require('prettier');

const args = require('yargs')
  .scriptName('tamago')
  .usage('$0 <command> [options]')
  .command('format <file>', 'Formats <file>', {})
  .command('compile <file>', 'Compiles <file>', {})
  .help()
  .version()
  .demandCommand(1)
  .argv;

const read = (f) => fs.readFileSync(f, 'utf8');

switch (args._[0]) {
  case 'format': {
    const source = read(args.file);
    const ast = Tamago.parse(source);
    const formatted = Tamago.prettyPrint(ast);
    console.log(formatted);
    break;
  }

  case 'compile': {
    const source = read(args.file);
    const ast = Tamago.parse(source);
    const js = Tamago.generate(ast);
    const pretty = prettier.format(js, { parser: 'babel' });
    console.log(pretty);
    break;
  }
  
  default:
    throw new Error(`Unknown command ${args._[0]}`);
}