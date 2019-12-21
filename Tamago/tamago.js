const fs = require('fs');
const path = require('path');
const Tamago = require('./build/package/Tamago');

const args = require('yargs')
  .scriptName('tamago')
  .usage('$0 <command> [options]')
  .command('format <file>', 'Formats <file>', {})
  .help()
  .version()
  .demandCommand(1)
  .argv;

const read = (f) => fs.readFileSync(f, 'utf8');

switch (args._[0]) {
  case 'ast': {
    const source = read(args.file);
    const ast = Tamago.parse(source);
    const formatted = Tamago.prettyPrint(ast);
    console.log(formatted);
  }
}