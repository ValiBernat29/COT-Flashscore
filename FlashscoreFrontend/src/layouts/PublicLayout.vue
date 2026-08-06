<script setup>
import { RouterView, RouterLink } from 'vue-router'
</script>

<template>
  <div class="min-h-screen bg-gray-100 text-gray-900">
    <nav class="bg-blue-600 text-white p-4 flex justify-between items-center shadow-md">
      <h1 class="text-xl font-bold">COT-SCORE</h1>
      <div class="space-x-4">
        <RouterLink to="/" class="hover:underline">Standings</RouterLink>
        <RouterLink to="/matches" class="hover:underline">Matches</RouterLink>
        <RouterLink to="/admin" class="text-blue-200 hover:text-white transition"
          >Admin Access</RouterLink
        >
      </div>
    </nav>
    <main class="p-6 max-w-5xl mx-auto">
      <RouterView />
    </main>
  </div>
</template>
