/** @type {import('jest').Config} */
module.exports = {
  testEnvironment: 'node',
  roots: ['<rootDir>/src'],
  transform: {
    '^.+\\.(ts|tsx)$': ['ts-jest', { tsconfig: '<rootDir>/tsconfig.jest.json' }]
  },
  // Use CommonJS for tests via tsconfig.jest.json; do not treat TS as ESM here.
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],
  moduleNameMapper: {
    '^~/(.*)$': '<rootDir>/src/$1'
  },
  collectCoverageFrom: [
    'src/utils/**/*.{ts,tsx}'
  ]
};
