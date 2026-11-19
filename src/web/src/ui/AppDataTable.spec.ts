import { screen, within } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'
import { renderUi, themeClasses } from '@/test/render-ui'
import AppDataTable from './AppDataTable.vue'

const columns = [
  { field: 'name', header: 'Name' },
  { field: 'city', header: 'City' },
]
const rows = [
  { id: '1', name: 'Acme Pty Ltd', city: 'Adelaide' },
  { id: '2', name: 'Globex', city: 'Canberra' },
]

describe('AppDataTable', () => {
  it('renders column headers and one row per item from an unstyled OpenVue table', () => {
    renderUi(AppDataTable, { props: { columns, rows, dataKey: 'id', caption: 'Organisations' } })

    const table = screen.getByRole('table', { name: 'Organisations' })
    expect(
      within(table)
        .getAllByRole('columnheader')
        .map((h) => h.textContent?.trim()),
    ).toEqual(['Name', 'City'])
    const bodyRows = within(table).getAllByRole('row').slice(1)
    expect(
      bodyRows.map((r) =>
        within(r)
          .getAllByRole('cell')
          .map((c) => c.textContent),
      ),
    ).toEqual([
      ['Acme Pty Ltd', 'Adelaide'],
      ['Globex', 'Canberra'],
    ])
  })

  it('styles the table through Tailwind pass-through classes only', () => {
    renderUi(AppDataTable, { props: { columns, rows, dataKey: 'id', caption: 'Organisations' } })

    const table = screen.getByRole('table', { name: 'Organisations' })
    expect(table).toHaveClass('min-w-full', 'text-sm')
    expect(themeClasses(table)).toEqual([])
  })
})
