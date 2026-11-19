import { fireEvent, screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'
import { renderUi, themeClasses } from '@/test/render-ui'
import AppDialog from './AppDialog.vue'

describe('AppDialog', () => {
  it('renders an open, labelled, unstyled OpenVue dialog with Tailwind pass-through classes', async () => {
    renderUi(AppDialog, {
      props: { header: 'Edit organisation', visible: true },
      slots: { default: '<p>Dialog body</p>' },
    })

    const dialog = await screen.findByRole('dialog', { name: 'Edit organisation' })
    expect(dialog).toHaveAttribute('data-pc-name', 'dialog')
    expect(dialog).toHaveClass('rounded-lg', 'bg-white')
    expect(themeClasses(dialog)).toEqual([])
    expect(screen.getByText('Dialog body')).toBeInTheDocument()
  })

  it('asks to close when the close button is pressed', async () => {
    const { emitted } = renderUi(AppDialog, {
      props: { header: 'Edit organisation', visible: true },
    })

    await fireEvent.click(await screen.findByRole('button', { name: 'Close' }))

    expect(emitted('update:visible')).toEqual([[false]])
  })
})
