<script setup lang="ts">
import { RouterView } from 'vue-router'
import HealthStatus from '@/shell/HealthStatus.vue'
import AppLayout from '@/ui/AppLayout.vue'
</script>

<template>
  <AppLayout app-name="CRM">
    <template #status>
      <HealthStatus />
    </template>
    <RouterView />
  </AppLayout>
</template>
