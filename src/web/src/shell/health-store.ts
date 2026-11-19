import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '@/api/client'

export const useHealthStore = defineStore('health', () => {
  const status = ref('checking')

  async function load() {
    const health = await apiFetch('/api/v1/health')
    status.value = health.status
  }

  return { status, load }
})
