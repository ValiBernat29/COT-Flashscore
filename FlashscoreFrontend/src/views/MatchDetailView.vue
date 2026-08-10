<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { useFixtureStore } from '../stores/fixtures'
import { useTeamStore } from '../stores/teams'
import { useLiveMatchStore } from '../stores/liveMatch'
import MatchLineupPitch from '../components/public/MatchLineupPitch.vue'

const route = useRoute()
const fixtureStore = useFixtureStore()
const teamStore = useTeamStore()
const liveMatchStore = useLiveMatchStore()

const matchId = Number(route.params.id) || route.params.id

const match = computed(
  () =>
    fixtureStore.fixtures.find((m) => String(m.id) === String(matchId))
)

const getTeamName = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.name ?? '—'

// ── Rosters ──────────────────────────────────────────────────
const homeRoster = ref([])
const awayRoster = ref([])
const loading = ref(true)

onMounted(async () => {
  if (!match.value) { loading.value = false; return }
  try {
    const [hr, ar] = await Promise.all([
      fetch(`http://localhost:5198/api/players/team/${match.value.homeTeamId}`).then(r => r.json()),
      fetch(`http://localhost:5198/api/players/team/${match.value.awayTeamId}`).then(r => r.json()),
    ])
    homeRoster.value = hr
    awayRoster.value = ar
  } catch (e) {
    console.error('Failed to fetch rosters', e)
  } finally {
    loading.value = false
  }
})

// ── Status helpers ────────────────────────────────────────────
const isLive = computed(() => match.value?.status === 'Live')
const isFinished = computed(() => match.value?.status === 'Finished')

const isActiveMatch = computed(
  () => isLive.value && String(liveMatchStore.activeMatchId) === String(matchId)
)

const liveMinute = computed(() =>
  liveMatchStore.matchPhase === 'HT' ? 'HT' : liveMatchStore.currentMinute + "'"
)

// Events: live store if active, else stored events on the fixture
const events = computed(() => {
  if (isActiveMatch.value) return liveMatchStore.events
  return match.value?.events ?? []
})

const hasLineup = computed(
  () => match.value?.homeLineup?.length > 0 || match.value?.awayLineup?.length > 0
)
</script>

<template>
  <div class="space-y-6">

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

    <!-- Not found -->
    <div v-if="!match" class="text-center py-20 text-gray-400 italic">
      Match not found.
    </div>

    <template v-else>

      <!-- ── Match Header Card ── -->
      <div
        class="bg-white rounded-xl shadow-md overflow-hidden border-t-4"
        :class="{
          'border-red-500': isLive,
          'border-gray-800': isFinished,
          'border-gray-300': !isLive && !isFinished,
        }"
      >
        <!-- Status strip -->
        <div
          class="flex items-center justify-center gap-3 py-2 text-xs font-bold uppercase tracking-widest"
          :class="{
            'bg-red-500 text-white': isLive,
            'bg-gray-800 text-white': isFinished,
            'bg-gray-100 text-gray-500': !isLive && !isFinished,
          }"
        >
          <span v-if="isLive" class="flex items-center gap-1.5">
            <span class="w-2 h-2 rounded-full bg-white animate-pulse inline-block"></span>
            Live
          </span>
          <span v-else-if="isFinished">Full Time</span>
          <span v-else>Scheduled</span>

          <!-- Live minute indicator -->
          <span
            v-if="isActiveMatch"
            class="bg-white/20 px-2 py-0.5 rounded-full text-white font-black"
          >
            {{ liveMinute }}
          </span>
        </div>

        <!-- Teams + Score -->
        <div class="grid grid-cols-3 items-center gap-4 px-8 py-8">
          <!-- Home -->
          <div class="text-right">
            <p class="text-2xl font-black text-gray-800 leading-tight">
              {{ getTeamName(match.homeTeamId) }}
            </p>
            <p class="text-xs text-gray-400 font-semibold mt-1 uppercase tracking-wide">Home</p>
          </div>

          <!-- Score -->
          <div class="text-center">
            <div
              class="text-5xl font-black tracking-tight leading-none"
              :class="isFinished ? 'text-gray-800' : isLive ? 'text-red-600' : 'text-gray-300'"
            >
              {{ match.homeScore }} – {{ match.awayScore }}
            </div>
          </div>

          <!-- Away -->
          <div class="text-left">
            <p class="text-2xl font-black text-gray-800 leading-tight">
              {{ getTeamName(match.awayTeamId) }}
            </p>
            <p class="text-xs text-gray-400 font-semibold mt-1 uppercase tracking-wide">Away</p>
          </div>
        </div>
      </div>

      <!-- ── Lineup Pitch ── -->
      <div v-if="hasLineup">
        <h2 class="text-lg font-bold text-gray-700 mb-3 flex items-center gap-2">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-blue-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="18" height="18" rx="2"/>
            <line x1="12" y1="3" x2="12" y2="21"/>
            <circle cx="12" cy="12" r="3"/>
          </svg>
          Starting Lineups
        </h2>
        <div v-if="loading" class="text-center py-10 text-gray-400 italic text-sm">
          Loading rosters…
        </div>
        <MatchLineupPitch
          v-else
          :match="match"
          :homeRoster="homeRoster"
          :awayRoster="awayRoster"
          :homeTeamName="getTeamName(match.homeTeamId)"
          :awayTeamName="getTeamName(match.awayTeamId)"
        />
      </div>

      <div v-else-if="!loading" class="bg-white rounded-xl shadow p-6 text-center text-gray-400 italic text-sm border border-dashed border-gray-200">
        Lineup not yet announced
      </div>

      <!-- ── Events ── -->
      <div class="bg-white rounded-xl shadow-md p-6">
        <h2 class="text-lg font-bold text-gray-700 mb-4 flex items-center gap-2">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-blue-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/>
            <polyline points="12 6 12 12 16 14"/>
          </svg>
          Match Events
        </h2>

        <ul v-if="events.length > 0" class="space-y-2">
          <li
            v-for="(event, i) in [...events].reverse()"
            :key="i"
            class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 hover:bg-gray-100 transition"
          >
            <!-- Minute badge -->
            <span class="w-12 text-center text-xs font-black text-blue-600 bg-blue-50 rounded-full py-1 shrink-0">
              {{ event.minute }}'
            </span>
            <!-- Icon -->
            <span class="text-lg">⚽</span>
            <!-- Text -->
            <span class="text-sm text-gray-700">
              Goal —
              <span class="font-bold text-gray-900">{{ getTeamName(event.teamId) }}</span>
            </span>
            <!-- Side indicator -->
            <span
              class="ml-auto text-xs font-bold px-2 py-0.5 rounded-full"
              :class="String(event.teamId) === String(match.homeTeamId)
                ? 'bg-blue-100 text-blue-700'
                : 'bg-red-100 text-red-700'"
            >
              {{ String(event.teamId) === String(match.homeTeamId) ? 'HOME' : 'AWAY' }}
            </span>
          </li>
        </ul>

        <p v-else class="text-sm text-gray-400 italic text-center py-6">
          {{ isLive ? 'No events yet — match in progress' : 'No events were recorded for this match.' }}
        </p>
      </div>

    </template>
  </div>
</template>
