<script setup lang="ts" generic="Row extends Record<string, unknown>">
import Column from 'openvue/column'
import DataTable from 'openvue/datatable'

export interface AppColumn<R> {
  field: keyof R & string
  header: string
}

defineProps<{
  columns: AppColumn<Row>[]
  rows: Row[]
  dataKey: keyof Row & string
  /** Accessible name of the table. */
  caption: string
}>()

const pt = {
  root: 'overflow-x-auto rounded-lg border border-slate-200',
  table: 'min-w-full divide-y divide-slate-200 text-sm',
  thead: 'bg-slate-50',
  tbody: 'divide-y divide-slate-100 bg-white',
  bodyRow: 'hover:bg-slate-50',
  emptyMessageCell: 'px-4 py-6 text-center text-slate-600',
}
const columnPt = {
  headerCell: 'px-4 py-3 text-left font-semibold text-slate-900',
  bodyCell: 'px-4 py-3 text-slate-800',
}
</script>

<template>
  <DataTable :value="rows" :data-key="dataKey" :table-props="{ 'aria-label': caption }" :pt="pt">
    <Column
      v-for="column in columns"
      :key="column.field"
      :field="column.field"
      :header="column.header"
      :pt="columnPt"
    />
  </DataTable>
</template>
