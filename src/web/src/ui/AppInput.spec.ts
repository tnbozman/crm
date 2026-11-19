import { fireEvent, screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'
import { renderUi, themeClasses } from '@/test/render-ui'
import AppInput from './AppInput.vue'

describe('AppInput', () => {
  it('renders an unstyled OpenVue input with a visible label and Tailwind pass-through classes', () => {
    renderUi(AppInput, { props: { label: 'Organisation name', modelValue: '' } })

    const input = screen.getByRole('textbox', { name: 'Organisation name' })
    expect(input).toHaveAttribute('data-pc-name', 'inputtext')
    expect(input).toHaveClass('rounded-md', 'border-slate-300')
    expect(themeClasses(input)).toEqual([])
  })

  it('emits the typed value for v-model', async () => {
    const { emitted } = renderUi(AppInput, {
      props: { label: 'Organisation name', modelValue: '' },
    })

    await fireEvent.update(screen.getByRole('textbox', { name: 'Organisation name' }), 'Acme')

    expect(emitted('update:modelValue')).toEqual([['Acme']])
  })
})
