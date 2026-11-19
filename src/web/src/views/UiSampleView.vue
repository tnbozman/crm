<script setup lang="ts">
import { ref } from 'vue'
import AppButton from '@/ui/AppButton.vue'
import AppDataTable from '@/ui/AppDataTable.vue'
import AppDialog from '@/ui/AppDialog.vue'
import AppInput from '@/ui/AppInput.vue'

// Development-only page that shows every shared UI wrapper (ADR-001). Synthetic data only.
const name = ref('')
const dialogOpen = ref(false)
const columns = [
  { field: 'name', header: 'Name' },
  { field: 'city', header: 'City' },
] as const
const rows = [
  { id: 'sample-1', name: 'Example Organisation One', city: 'Adelaide' },
  { id: 'sample-2', name: 'Example Organisation Two', city: 'Canberra' },
]
</script>

<template>
  <div class="flex max-w-3xl flex-col gap-6">
    <h1 class="text-2xl font-semibold">UI sample</h1>
    <AppInput v-model="name" label="Organisation name" />
    <div>
      <AppButton label="Open dialog" @click="dialogOpen = true" />
    </div>
    <AppDataTable
      :columns="[...columns]"
      :rows="rows"
      data-key="id"
      caption="Sample organisations"
    />
    <AppDialog v-model:visible="dialogOpen" header="Sample dialog">
      <p>Dialog content for {{ name || 'no organisation' }}.</p>
      <template #footer>
        <AppButton label="Done" @click="dialogOpen = false" />
      </template>
    </AppDialog>
  </div>
</template>
