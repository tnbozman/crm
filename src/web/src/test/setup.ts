import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/vue'
import { afterAll, afterEach, beforeAll } from 'vitest'
import { server } from './server'

beforeAll(() => server.listen({ onUnhandledFrame: 'error' }))
afterEach(() => {
  // Testing Library only auto-cleans up when Vitest globals are enabled; they are not.
  cleanup()
  server.resetHandlers()
})
afterAll(() => server.close())
