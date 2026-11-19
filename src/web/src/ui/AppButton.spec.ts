import { screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'
import { renderUi, themeClasses } from '@/test/render-ui'
import AppButton from './AppButton.vue'

describe('AppButton', () => {
  it('renders an unstyled OpenVue button styled by Tailwind pass-through classes', () => {
    renderUi(AppButton, { props: { label: 'Save' } })

    const button = screen.getByRole('button', { name: 'Save' })
    expect(button).toHaveAttribute('data-pc-name', 'button')
    expect(button).toHaveClass('bg-blue-700', 'text-white')
    expect(themeClasses(button)).toEqual([])
  })
})
