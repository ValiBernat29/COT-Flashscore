<script setup>
import { ref } from 'vue'
import { useTeamStore } from '../../stores/teams'
import { useFixtureStore } from '../../stores/fixtures'
import { useLiveMatchStore } from '../../stores/liveMatch'

const emit = defineEmits(['view-live'])
const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()
const liveMatchStore = useLiveMatchStore()

const expandedMatches = ref({})

const toggleEvents = (matchId) => {
  expandedMatches.value[matchId] = !expandedMatches.value[matchId]
}

const getTeamName = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.name || 'Deleted Team'

const getTeamLogo = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.logoUrl || ''

const handleStartMatch = async (matchId) => {
  if (liveMatchStore.activeMatchId) {
    alert('You must conclude or cancel the current running match first.')
    return
  }
  liveMatchStore.activeMatchId = matchId
  emit('view-live')
}

const handleResumeMatch = async (matchId) => {
  if (liveMatchStore.activeMatchId && String(liveMatchStore.activeMatchId) !== String(matchId)) {    alert('Another match is currently loaded. Cancel it first.')
    return
  }
  liveMatchStore.activeMatchId = matchId
  emit('view-live')
}
</script>

<template>
  <div class="space-y-6">
    <h2 class="text-2xl font-bold text-emerald-400">Match Controller</h2>
    <h3 class="text-lg text-white mb-4">Select a match to manage:</h3>

    <div v-for="group in fixtureStore.matchesByMatchday" :key="group.matchday" class="mb-6">
      <h4 class="text-white font-bold mb-3 border-b border-slate-600 pb-1 text-lg">
        Matchday {{ group.matchday }}
      </h4>
      <div class="space-y-3">
        <div
          v-for="match in group.matches"
          :key="match.id"
          class="bg-slate-700 p-4 rounded border border-slate-600 flex flex-col"
        >
          <div class="flex justify-between items-center">
            <span class="text-white font-medium text-lg flex items-center gap-2"
              >
              <img v-if="getTeamLogo(match.homeTeamId)" :src="getTeamLogo(match.homeTeamId)" class="w-6 h-6 object-contain" />
              {{ getTeamName(match.homeTeamId) }}
              <span class="text-slate-400 mx-1">{{ match.homeScore }} - {{ match.awayScore }}</span>
              {{ getTeamName(match.awayTeamId) }}
              <img v-if="getTeamLogo(match.awayTeamId)" :src="getTeamLogo(match.awayTeamId)" class="w-6 h-6 object-contain" />
              </span
            >

            <div v-if="match.status === 'Finished'" class="flex flex-col items-end">
              <span class="text-slate-400 font-bold uppercase tracking-wider mb-1">Finished</span>
              <button
                @click="toggleEvents(match.id)"
                class="text-xs text-blue-400 hover:text-blue-300 font-semibold"
              >
                {{ expandedMatches[match.id] ? 'Hide Events ▲' : 'Show Events ▼' }}
              </button>
            </div>

            <button
              v-else-if="match.status === 'Live'"
              @click="handleResumeMatch(match.id)"
              class="px-4 py-2 bg-amber-500 hover:bg-amber-400 text-white rounded font-semibold transition flex gap-2 items-center shadow-lg animate-pulse"
            >
              <span class="w-2 h-2 bg-white rounded-full"></span> Resume Live
            </button>

            <button
              v-else-if="match.status === 'Scheduled'"
              @click="handleStartMatch(match.id)"
              :disabled="liveMatchStore.activeMatchId !== null"
              :class="
                liveMatchStore.activeMatchId !== null
                  ? 'opacity-50 cursor-not-allowed bg-slate-600'
                  : 'bg-emerald-600 hover:bg-emerald-500'
              "
              class="px-4 py-2 text-white rounded font-semibold transition"
            >
              Start Live
            </button>
          </div>

          <div
            v-if="match.status === 'Finished' && expandedMatches[match.id]"
            class="mt-4 bg-slate-800 rounded p-3 border border-slate-600"
          >
            <ul class="space-y-2">
              <li
                v-for="(event, index) in match.events || []"
                :key="index"
                class="text-sm flex items-center text-slate-300"
              >
                <span class="font-bold text-emerald-400 mr-2">{{ event.minute }}'</span>
                <span
                  >⚽ Goal by
                  <span class="font-bold text-white">{{ getTeamName(event.teamId) }}</span></span
                >
              </li>
              <li
                v-if="!(match.events && match.events.length)"
                class="text-sm text-slate-500 italic"
              >
                No events logged.
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
