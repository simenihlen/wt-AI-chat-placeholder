module.exports = {
  root: true,
  env: {
    node: true,
    es2022: true
  },
  parser: '@typescript-eslint/parser',
  plugins: [
    '@typescript-eslint'
  ],
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended'
  ],
  rules: {
    'quotes': ['error', 'single', { 'allowTemplateLiterals': true, 'avoidEscape': true }], // enforces single-quotes, backticks and dobule quotes within single-quotes are allowed.
    'no-trailing-spaces': 'error', // enforces that lines of code should not have any trailing whitespace.
    'no-whitespace-before-property': 'error', // disallows whitespace between an object and its property. Like: obj .property and obj. property
    'array-bracket-newline': ['error', 'consistent'], // enforces consistent line breaks inside array brackets.
    'brace-style': ['error', '1tbs', { 'allowSingleLine': true }], // enforces a specific brace style for code blocks in your JavaScript code.
    'semi': 'error', // enforces the consistent use of semicolons at the end of statements in your code.
    'semi-spacing': 'error', // enforces consistent spacing around semicolons. Like: var a = 0; and var a = 0 ;.
    'semi-style': ['error', 'last'], // enforces that semicolons should be at the end of statements, not at the beginning.
    'func-call-spacing': ['error', 'never'], // enforces that there should be no space between a function name and the parentheses when calling the function.
    'key-spacing': ['error', { 'beforeColon': false, 'afterColon': true, 'mode': 'strict' }], // enforces consistent spacing around colons in object literals.
    'keyword-spacing': ['error', { 'before': true, 'after': true }], // enforces consistent spacing around keywords in your JavaScript code.
    'comma-dangle': ['error', 'never'], // disallows trailing commas in object literals, array literals, function calls, and function declarations.
    'comma-spacing': ['error', { 'before': false, 'after': true }], // enforces consistent spacing around commas in your JavaScript code.
    'comma-style': ['error', 'last'], // enforces that commas in multi-line lists (arrays, objects, function parameters) should be placed after the element, not before.
    '@typescript-eslint/explicit-module-boundary-types': 'off',
    '@typescript-eslint/no-explicit-any': 'off',
    '@typescript-eslint/no-unused-vars': ['error', { 'argsIgnorePattern': '^(err|event)' }] // ignore unused variables, except variables starting with: 'err' and 'event'.
  }
};
