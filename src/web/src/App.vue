<script setup lang="ts">
import { RouterView } from 'vue-router'
import HealthStatus from '@/shell/HealthStatus.vue'
</script>

<template>
  <main>
    <RouterView />
    <HealthStatus />
  </main>
</template>
