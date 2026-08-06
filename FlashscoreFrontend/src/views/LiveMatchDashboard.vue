<script setup>
import { ref } from 'vue'

// Import our newly created components
import TeamModal from '../components/admin/TeamModal.vue'
import FixtureModal from '../components/admin/FixtureModal.vue'
import AdminMatchList from '../components/admin/AdminMatchList.vue'
import LiveMatchBoard from '../components/admin/LiveMatchBoard.vue'

// Component State
const isTeamModalOpen = ref(false)
const isFixtureModalOpen = ref(false)
const isViewingMatch = ref(false)
</script>

<template>
  <div class="space-y-6">
    <!-- Top Bar -->
    <div v-if="!isViewingMatch" class="flex justify-end gap-4">
      <button
        @click="isTeamModalOpen = true"
        class="px-6 py-2 bg-slate-700 hover:bg-slate-600 border border-slate-600 text-white rounded font-bold transition shadow flex items-center gap-2"
      >
        <span>⚙️</span> Teams
      </button>
      <button
        @click="isFixtureModalOpen = true"
        class="px-6 py-2 bg-slate-700 hover:bg-slate-600 border border-slate-600 text-white rounded font-bold transition shadow flex items-center gap-2"
      >
        <span>📅</span> Fixtures
      </button>
    </div>

    <!-- Modals -->
    <TeamModal v-if="isTeamModalOpen" @close="isTeamModalOpen = false" />
    <FixtureModal v-if="isFixtureModalOpen" @close="isFixtureModalOpen = false" />

    <!-- Main Controller Area -->
    <div class="bg-slate-800 rounded-lg shadow-xl p-6 border border-slate-700">
      <LiveMatchBoard v-if="isViewingMatch" @close="isViewingMatch = false" />
      <AdminMatchList v-else @view-live="isViewingMatch = true" />
    </div>
  </div>
</template>
