import { fireEvent, screen } from '@testing-library/vue'
import { http, HttpResponse } from 'msw'
import { beforeEach, describe, expect, it } from 'vitest'
import { axeViolations } from '@/test/axe'
import { renderApp } from '@/test/render-app'
import { server } from '@/test/server'

describe('UI sample page', () => {
  beforeEach(() => {
    server.use(http.get('/api/v1/health', () => HttpResponse.json({ status: 'ok' })))
  })

  it('shows every UI wrapper and has no axe violations', async () => {
    const { container } = await renderApp('/ui-sample')

    expect(await screen.findByRole('heading', { name: 'UI sample' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Organisation name' })).toBeInTheDocument()
    expect(screen.getByRole('table', { name: 'Sample organisations' })).toBeInTheDocument()
    expect(await axeViolations(container)).toEqual([])
  })

  it('has no axe violations with the dialog open', async () => {
    await renderApp('/ui-sample')

    await fireEvent.click(await screen.findByRole('button', { name: 'Open dialog' }))

    expect(await screen.findByRole('dialog', { name: 'Sample dialog' })).toBeInTheDocument()
    expect(await axeViolations(document.body)).toEqual([])
  })
})
