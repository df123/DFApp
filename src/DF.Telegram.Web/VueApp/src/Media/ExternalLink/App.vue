<template>
    <button @click="count++">Count2 is: {{ count }}</button>
</template>

<script setup lang="ts">
    import { ref } from 'vue'
    const count = ref(0)
</script>

<style scoped>
    button {
        font-weight: bold;
    }
</style>