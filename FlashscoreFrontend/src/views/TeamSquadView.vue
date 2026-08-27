<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { useTeamStore } from '../stores/teams'

const route = useRoute()
const teamStore = useTeamStore()

const teamId = route.params.id
const players = ref([])
const loading = ref(true)
const error = ref(false)

const team = computed(() =>
  teamStore.teams.find((t) => String(t.id) === String(teamId))
)

onMounted(async () => {
  try {
    const res = await fetch(`http://localhost:5198/api/players/team/${teamId}`)
    if (!res.ok) throw new Error('Failed')
    players.value = await res.json()
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
})

const positionOrder = { GK: 0, DEF: 1, MID: 2, FWD: 3 }
const positionLabel = { GK: 'Goalkeepers', DEF: 'Defenders', MID: 'Midfielders', FWD: 'Forwards' }
const positionColor = {
  GK:  { bg: 'bg-amber-100',   text: 'text-amber-700',   border: 'border-amber-300',   dot: 'bg-amber-400'   },
  DEF: { bg: 'bg-blue-100',    text: 'text-blue-700',    border: 'border-blue-300',    dot: 'bg-blue-500'    },
  MID: { bg: 'bg-emerald-100', text: 'text-emerald-700', border: 'border-emerald-300', dot: 'bg-emerald-500' },
  FWD: { bg: 'bg-red-100',     text: 'text-red-700',     border: 'border-red-300',     dot: 'bg-red-500'     },
}

const grouped = computed(() => {
  const groups = {}
  for (const p of players.value) {
    const pos = p.position || 'MID'
    if (!groups[pos]) groups[pos] = []
    groups[pos].push(p)
  }
  return Object.entries(groups).sort(([a], [b]) => (positionOrder[a] ?? 99) - (positionOrder[b] ?? 99))
})
</script>

<template>
  <div class="space-y-8">

    <!-- Back link -->
    <RouterLink
      to="/matches"
      class="inline-flex items-center gap-2 text-sm font-semibold text-blue-600 hover:text-blue-800 transition group"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 transition-transform group-hover:-translate-x-0.5" viewBox="0 0 20 20" fill="currentColor">
        <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd"/>
      </svg>
      Back to Matches
    </RouterLink>

    <!-- Team header hero -->
    <div class="squad-hero rounded-2xl overflow-hidden shadow-xl relative">
      <div class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 opacity-95"></div>

      <!-- Faint blurred crest in background -->
      <div v-if="team?.logoUrl" class="absolute inset-0 flex items-center justify-end pr-8 pointer-events-none overflow-hidden">
        <img :src="team.logoUrl" class="w-72 h-72 object-contain opacity-[0.04] blur-sm scale-125" />
      </div>

      <div class="relative z-10 flex flex-col sm:flex-row items-center gap-6 px-10 py-10">
        <!-- Crest ring -->
        <div class="flex items-center justify-center rounded-full bg-white/10 backdrop-blur-sm p-3 shadow-2xl border border-white/20 w-28 h-28 flex-shrink-0">
          <img
            v-if="team?.logoUrl"
            :src="team.logoUrl"
            :alt="team?.name"
            class="w-20 h-20 object-contain drop-shadow-lg"
          />
          <div v-else class="w-20 h-20 flex items-center justify-center text-white text-4xl font-black">
            {{ team?.name?.[0] ?? '?' }}
          </div>
        </div>

        <div class="text-center sm:text-left">
          <p class="text-white/50 text-xs font-bold uppercase tracking-widest mb-1">Squad</p>
          <h1 class="text-4xl font-black text-white leading-tight tracking-tight">
            {{ team?.name ?? 'Team' }}
          </h1>
          <p class="text-white/60 mt-2 text-sm font-medium">
            {{ loading ? '…' : players.length }} Players registered
          </p>
        </div>

        <!-- Position legend chips -->
        <div v-if="!loading && grouped.length" class="sm:ml-auto flex flex-wrap gap-2 justify-center sm:justify-end">
          <span
            v-for="([pos]) in grouped"
            :key="pos"
            :class="[positionColor[pos]?.bg ?? 'bg-gray-100', positionColor[pos]?.text ?? 'text-gray-700', positionColor[pos]?.border ?? 'border-gray-300']"
            class="px-3 py-1 rounded-full text-xs font-bold border"
          >
            {{ positionLabel[pos] ?? pos }}
          </span>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-24 gap-4">
      <div class="w-12 h-12 rounded-full border-4 border-blue-200 border-t-blue-600 animate-spin"></div>
      <p class="text-gray-400 text-sm italic">Loading squad…</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-xl p-8 text-center text-red-500 italic">
      Could not load the squad. Please try again later.
    </div>

    <!-- Empty -->
    <div v-else-if="players.length === 0" class="bg-white rounded-xl shadow border border-dashed border-gray-200 p-12 text-center text-gray-400 italic">
      No players registered for this team yet.
    </div>

    <!-- Grouped sections -->
    <template v-else>
      <div v-for="([pos, posPlayers]) in grouped" :key="pos" class="space-y-3">

        <!-- Section heading -->
        <div class="flex items-center gap-3 mb-1">
          <span :class="positionColor[pos]?.dot ?? 'bg-gray-400'" class="w-2.5 h-2.5 rounded-full inline-block flex-shrink-0"></span>
          <h2 class="text-sm font-bold text-gray-500 uppercase tracking-widest">
            {{ positionLabel[pos] ?? pos }}
          </h2>
          <div class="flex-1 h-px bg-gray-100"></div>
          <span class="text-xs text-gray-400 font-semibold">{{ posPlayers.length }}</span>
        </div>

        <!-- Player grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          <div
            v-for="player in posPlayers.slice().sort((a, b) => a.number - b.number)"
            :key="player.id"
            class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 flex items-center gap-4 hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 group cursor-default"
          >
            <!-- Avatar -->
            <div class="relative flex-shrink-0">
              <img
                v-if="player.photoUrl"
                :src="player.photoUrl"
                :alt="player.name"
                class="w-14 h-14 rounded-full object-cover bg-gray-100 border-2 border-white shadow"
              />
              <div
                v-else
                :class="[positionColor[pos]?.bg ?? 'bg-gray-100', positionColor[pos]?.text ?? 'text-gray-600']"
                class="w-14 h-14 rounded-full flex items-center justify-center text-xl font-black border-2 border-white shadow"
              >
                {{ player.number }}
              </div>
              <span
                :class="positionColor[pos]?.dot ?? 'bg-gray-400'"
                class="absolute bottom-0 right-0 w-3.5 h-3.5 rounded-full border-2 border-white shadow"
              ></span>
            </div>

            <!-- Info -->
            <div class="min-w-0 flex-1">
              <p class="font-bold text-gray-900 truncate group-hover:text-blue-600 transition-colors">
                {{ player.name }}
              </p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-xs text-gray-400 font-semibold">#{{ player.number }}</span>
                <span
                  :class="[positionColor[pos]?.bg ?? 'bg-gray-100', positionColor[pos]?.text ?? 'text-gray-600', positionColor[pos]?.border ?? 'border-gray-200']"
                  class="text-xs font-bold px-2 py-0.5 rounded-full border"
                >
                  {{ player.position }}
                </span>
              </div>
            </div>
          </div>
        </div>

      </div>
    </template>

  </div>
</template>

<style scoped>
.squad-hero {
  min-height: 175px;
}
</style>
