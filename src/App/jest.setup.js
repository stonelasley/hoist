// Mock Expo Winter runtime globals
global.__ExpoImportMetaRegistry = new Map();

// Mock __DEV__
global.__DEV__ = true;

// Mock structuredClone if not available
if (typeof global.structuredClone === 'undefined') {
  global.structuredClone = (obj) => JSON.parse(JSON.stringify(obj));
}

// Setup testing library
require('@testing-library/jest-native/extend-expect');
