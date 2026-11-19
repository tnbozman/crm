<script setup lang="ts">
import { useId } from 'vue'
import InputText from 'openvue/inputtext'

defineProps<{ label: string }>()
const model = defineModel<string>({ required: true })
const id = useId()

const pt = {
  root: 'block w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-500 focus:border-blue-700 focus:outline-2 focus:outline-blue-700',
}
</script>

<template>
  <div class="flex flex-col gap-1">
    <label :for="id" class="text-sm font-medium text-slate-800">{{ label }}</label>
    <InputText :id="id" v-model="model" :pt="pt" />
  </div>
</template>
