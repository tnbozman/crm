import { render, type RenderOptions } from '@testing-library/vue'
import OpenVue from 'openvue/config'
import { openVueOptions } from '@/ui/openvue'

/** Renders a component with OpenVue installed exactly as the app installs it. */
export function renderUi<C>(component: C, options: RenderOptions<C> = {}) {
  return render(component, {
    ...options,
    global: {
      ...options.global,
      plugins: [[OpenVue, openVueOptions], ...(options.global?.plugins ?? [])],
    },
  })
}

/** Class names that would come from an OpenVue theme rather than our pass-through. */
export function themeClasses(element: Element): string[] {
  return [...element.classList].filter((c) => c.startsWith('p-'))
}
