<template>
  <span v-if="sortField === field">
    <svg
      v-if="sortAsc"
      xmlns="http://www.w3.org/2000/svg"
      class="inline-block w-4 h-4 ml-1"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
    >
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
    </svg>
    <svg
      v-else
      xmlns="http://www.w3.org/2000/svg"
      class="inline-block w-4 h-4 ml-1"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
    >
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
    </svg>
  </span>
</template>

<script lang="ts" setup>
defineProps<{
  field: string;
  sortField: string;
  sortAsc: boolean;
}>();
</script>
