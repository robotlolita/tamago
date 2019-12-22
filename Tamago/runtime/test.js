const { assert } = require('./assert');

class TestCase {
  constructor(module, description, test) {
    this._module = module;
    this._description = description;
    this._test = test;
  }
}

module.exports = {
  TestCase
};
