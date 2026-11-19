<script setup lang="ts">
defineProps<{ appName: string }>()
</script>

<template>
  <div class="flex min-h-screen flex-col bg-slate-50 text-slate-900">
    <a
      href="#main"
      class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:rounded-md focus:bg-white focus:px-3 focus:py-2 focus:shadow"
    >
      Skip to main content
    </a>
    <header class="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3">
      <span class="text-lg font-semibold">{{ appName }}</span>
      <slot name="status" />
    </header>
    <main id="main" class="flex-1 px-6 py-6" tabindex="-1">
      <slot />
    </main>
  </div>
</template>
