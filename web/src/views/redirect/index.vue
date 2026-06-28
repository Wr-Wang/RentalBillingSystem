<template>
  <router-view />
</template>

<script setup>
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

onMounted(() => {
  const path = route.params.path
  if (path) {
    router.replace('/' + (Array.isArray(path) ? path.join('/') : path))
  } else {
    router.replace('/dashboard')
  }
})
</script>
