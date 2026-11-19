import { globalIgnores } from 'eslint/config'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import pluginVue from 'eslint-plugin-vue'
import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'

export default defineConfigWithVueTs(
  {
    name: 'crm/files-to-lint',
    files: ['**/*.{ts,mts,vue}'],
  },

  globalIgnores(['**/dist/**', '**/coverage/**', 'src/api/schema.d.ts']),

  pluginVue.configs['flat/recommended'],
  vueTsConfigs.recommended,

  {
    name: 'crm/strictness',
    rules: {
      '@typescript-eslint/no-explicit-any': 'error',
    },
  },

  // Prettier owns formatting; ESLint does not report style.
  skipFormatting,
)
