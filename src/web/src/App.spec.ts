import { screen, waitFor } from '@testing-library/vue'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { renderApp } from '@/test/render-app'
import { server } from '@/test/server'

describe('App', () => {
  it('shows the health status returned by the API', async () => {
    server.use(http.get('/api/v1/health', () => HttpResponse.json({ status: 'ok' })))

    await renderApp()

    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('API status: ok'))
  })

  it('shows an error state when the API fails', async () => {
    server.use(
      http.get('/api/v1/health', () =>
        HttpResponse.json({ title: 'Service Unavailable', status: 503 }, { status: 503 }),
      ),
    )

    await renderApp()

    expect(await screen.findByRole('alert')).toHaveTextContent('API status: unavailable')
  })
})
