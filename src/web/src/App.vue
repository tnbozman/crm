<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'

const status = ref('checking')

onMounted(async () => {
  const response = await fetch(new URL('/api/v1/health', window.location.origin))
  const body = (await response.json()) as { status: string }
  status.value = body.status
})
</script>

<template>
  <main>
    <RouterView />
    <p role="status">API status: {{ status }}</p>
  </main>
</template>
