import type { PrimeVueConfiguration } from 'openvue/config'

/**
 * OpenVue runs in unstyled mode: no theme CSS, no p-* classes. Every wrapper in src/ui
 * styles its component with Tailwind classes through the `pt` (pass-through) prop. ADR-001.
 */
export const openVueOptions = {
  unstyled: true,
} satisfies PrimeVueConfiguration
