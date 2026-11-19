<script setup lang="ts">
import Dialog from 'openvue/dialog'

defineProps<{ header: string }>()
const visible = defineModel<boolean>('visible', { required: true })

const pt = {
  mask: 'fixed inset-0 flex items-center justify-center bg-slate-900/50 p-4',
  root: 'flex max-h-[90vh] w-full max-w-lg flex-col rounded-lg bg-white shadow-xl',
  header: 'flex items-center justify-between border-b border-slate-200 px-6 py-4',
  title: 'text-lg font-semibold text-slate-900',
  pcCloseButton: {
    root: 'rounded-md p-1 text-slate-600 hover:bg-slate-100 hover:text-slate-900 focus-visible:outline-2 focus-visible:outline-blue-700',
  },
  content: 'overflow-y-auto px-6 py-4 text-sm text-slate-800',
  footer: 'flex justify-end gap-2 border-t border-slate-200 px-6 py-4',
}
</script>

<template>
  <Dialog v-model:visible="visible" :header="header" modal :pt="pt">
    <slot />
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>
