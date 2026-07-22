<script setup lang="ts">
import { onMounted } from "vue";
import { init } from "./bridge";

onMounted(() => init());
</script>

<template>
  <router-view />
</template>
