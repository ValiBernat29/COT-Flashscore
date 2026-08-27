<script setup>
import { ref } from 'vue'

import TeamModal from '../components/admin/TeamModal.vue'
import FixtureModal from '../components/admin/FixtureModal.vue'
import AdminMatchList from '../components/admin/AdminMatchList.vue'
import LiveMatchBoard from '../components/admin/LiveMatchBoard.vue'
import { useApiSyncStore } from '../stores/apiSync'
import { useTeamStore } from '../stores/teams'

const isTeamModalOpen = ref(false)
const isFixtureModalOpen = ref(false)
const isViewingMatch = ref(false)

const syncStore = useApiSyncStore()
const teamStore = useTeamStore()

const handleSync = async () => {
  await syncStore.syncTeams()
  await teamStore.fetchTeams()
}
</script>

<template>
  <div class="space-y-6">
    <!-- API-Football Sync Panel -->
    <div class="bg-slate-800 rounded-lg border border-slate-700 overflow-hidden">
      <div class="p-4 border-b border-slate-700 flex items-center gap-3">
        <span class="text-xl">⚡</span>
        <h3 class="text-base font-bold text-white">API-Football Sync</h3>
        <span class="text-xs text-slate-400 ml-auto">Liga 1 Romania · Season 2024</span>
      </div>
      <div class="p-4 flex flex-wrap items-center gap-4">
        <button
          @click="handleSync"
          :disabled="syncStore.isSyncing"
          class="flex items-center gap-2 px-5 py-2 bg-emerald-600 hover:bg-emerald-500 disabled:bg-slate-600 disabled:cursor-not-allowed text-white font-bold rounded-lg transition shadow-lg shadow-emerald-500/20"
        >
          <svg v-if="syncStore.isSyncing" class="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
          </svg>
          <span v-else>🔄</span>
          {{ syncStore.isSyncing ? 'Syncing teams & players…' : 'Sync Teams & Players' }}
        </button>

        <div v-if="syncStore.syncMessage" class="flex items-center gap-2 text-emerald-400 font-semibold text-sm">
          <span>✅</span> {{ syncStore.syncMessage }}
          <span v-if="syncStore.lastSyncTime" class="text-slate-400 font-normal">at {{ syncStore.lastSyncTime }}</span>
        </div>
        <div v-if="syncStore.syncError" class="flex items-center gap-2 text-red-400 font-semibold text-sm">
          <span>❌</span> {{ syncStore.syncError }}
        </div>
        <div v-if="!syncStore.syncMessage && !syncStore.syncError && !syncStore.isSyncing" class="text-slate-500 text-sm italic">
          Pulls all Liga 1 teams, logos and player squads into your database. Safe to re-run.
        </div>
      </div>
    </div>

    <div class="flex justify-end gap-4">
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

    <TeamModal v-if="isTeamModalOpen" @close="isTeamModalOpen = false" />
    <FixtureModal v-if="isFixtureModalOpen" @close="isFixtureModalOpen = false" />

    <div class="bg-slate-800 rounded-lg shadow-xl p-6 border border-slate-700">
      <LiveMatchBoard v-if="isViewingMatch" @close="isViewingMatch = false" />
      <AdminMatchList v-else @view-live="isViewingMatch = true" />
    </div>
  </div>
</template>
