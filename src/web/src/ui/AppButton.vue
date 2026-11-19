<script setup lang="ts">
import Button from 'openvue/button'

const { type = 'button' } = defineProps<{
  label: string
  type?: 'button' | 'submit'
}>()

const pt = {
  root: 'inline-flex items-center gap-2 rounded-md bg-blue-700 px-4 py-2 text-sm font-medium text-white hover:bg-blue-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700 disabled:opacity-50',
}
</script>

<template>
  <Button :label="label" :type="type" :pt="pt" />
</template>
