import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '@/api/client'

export const useHealthStore = defineStore('health', () => {
  const status = ref('checking')
  const failed = ref(false)

  async function load() {
    try {
      const health = await apiFetch('/api/v1/health')
      status.value = health.status
      failed.value = false
    } catch {
      status.value = 'unavailable'
      failed.value = true
    }
  }

  return { status, failed, load }
})
