<script setup lang="ts">
import { onMounted } from 'vue'
import { useHealthStore } from './health-store'

const health = useHealthStore()

onMounted(() => health.load())
</script>

<template>
  <p role="status">API status: {{ health.status }}</p>
</template>
