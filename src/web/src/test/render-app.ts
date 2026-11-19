import { render } from '@testing-library/vue'
import OpenVue from 'openvue/config'
import { createPinia } from 'pinia'
import { createMemoryHistory } from 'vue-router'
import App from '@/App.vue'
import { createAppRouter } from '@/router'
import { openVueOptions } from '@/ui/openvue'

/** Renders the whole app shell at the given route with a fresh router and store. */
export async function renderApp(route = '/') {
  const router = createAppRouter(createMemoryHistory())
  await router.push(route)
  await router.isReady()
  return render(App, { global: { plugins: [router, createPinia(), [OpenVue, openVueOptions]] } })
}
