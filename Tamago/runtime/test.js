const { assert } = require('./assert');

class TestCase {
  constructor(module, description, test) {
    this._module = module;
    this._description = description;
    this._test = test;
  }

  run() {
    const fn = this._test;
    try {
      fn();
      console.log(`[OK] ${test.description}`);
      return 0;
    } catch (error) {
      console.log(`[ERROR] ${test.module} - ${test.description}`);
      console.log(error.stack);
      console.log("---");
      return 1;
    }
  }
}

module.exports = {
  TestCase
};
