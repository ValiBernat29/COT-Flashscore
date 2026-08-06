<script setup>
import { RouterView, RouterLink } from 'vue-router'
</script>

<template>
  <div class="min-h-screen bg-slate-900 text-gray-100">
    <nav
      class="bg-slate-800 p-4 border-b border-slate-700 flex justify-between items-center shadow-lg"
    >
      <h1 class="text-xl font-bold text-emerald-400">Match Controller</h1>
      <div class="space-x-4">
        <RouterLink to="/admin" class="hover:text-emerald-400 transition"
          >Live Dashboard</RouterLink
        >
        <router-link to="/admin/rosters" class="hover:text-blue-400 transition"
          >Squad Management</router-link
        >
        <RouterLink to="/" class="text-slate-400 hover:text-white transition"
          >Exit to Public</RouterLink
        >
      </div>
    </nav>
    <main class="p-6 max-w-5xl mx-auto">
      <RouterView />
    </main>
  </div>
</template>
