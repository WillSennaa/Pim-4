// https://docs.expo.dev/guides/using-eslint/
const { defineConfig } = require('eslint/config');
const expoConfig = require('eslint-config-expo/flat');

module.exports = defineConfig([
  expoConfig,
  { ignores: ['dist/*'] },
  {
    // O resolvedor do eslint-plugin-import não entende o campo "exports"
    // com curinga do pacote de ícones; o Metro (empacotador do app) entende.
    rules: { 'import/no-unresolved': ['error', { ignore: ['^@tabler/icons-react-native/'] }] }
  }
]);
