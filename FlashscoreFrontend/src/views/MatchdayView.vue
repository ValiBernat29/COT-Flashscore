<script setup>
import { ref } from 'vue'
import { useFixtureStore } from '../stores/fixtures'
import { useTeamStore } from '../stores/teams'
import { useLiveMatchStore } from '../stores/liveMatch'

const fixtureStore = useFixtureStore()
const teamStore = useTeamStore()
const liveMatchStore = useLiveMatchStore()

const getTeamName = (id) => teamStore.teams.find((t) => String(t.id) === String(id))?.name

// An object to track which finished matches have their events expanded
const expandedMatches = ref({})

const toggleEvents = (matchId) => {
  expandedMatches.value[matchId] = !expandedMatches.value[matchId]
}
</script>

<template>
  <div class="space-y-8">
    <h2 class="text-3xl font-bold text-gray-800">Matchday Schedule</h2>
    <div v-if="fixtureStore.matchesByMatchday.length === 0" class="text-gray-500 italic">
      No matches scheduled yet.
    </div>

    <div v-for="group in fixtureStore.matchesByMatchday" :key="group.matchday" class="space-y-4">
      <div class="border-b-2 border-blue-600 pb-2 mb-4">
        <h3 class="text-xl font-bold text-gray-700 uppercase tracking-wide">
          Matchday {{ group.matchday }}
        </h3>
      </div>

      <div class="grid gap-4">
        <div
          v-for="match in group.matches"
          :key="match.id"
          class="bg-white rounded-lg shadow p-4 border-l-4"
          :class="{
            'border-red-500': match.status === 'Live',
            'border-gray-800': match.status === 'Finished',
            'border-gray-300': match.status === 'Scheduled',
          }"
        >
          <div class="flex justify-between items-center mb-2">
            <div class="flex items-center gap-2">
              <span
                v-if="match.status === 'Live'"
                class="text-sm font-bold text-red-500 animate-pulse uppercase tracking-wider"
                >Live</span
              >
              <span
                v-else-if="match.status === 'Finished'"
                class="text-sm font-bold text-gray-800 uppercase tracking-wider"
                >FT</span
              >
              <span v-else class="text-sm font-semibold text-gray-500 uppercase tracking-wider"
                >Scheduled</span
              >

              <span
                v-if="match.status === 'Live' && liveMatchStore.activeMatchId === match.id"
                class="text-sm font-bold px-2 py-0.5 rounded bg-gray-100"
                :class="liveMatchStore.matchPhase === 'HT' ? 'text-amber-500' : 'text-emerald-600'"
              >
                {{ liveMatchStore.matchPhase === 'HT' ? 'HT' : liveMatchStore.currentMinute + "'" }}
              </span>
            </div>
          </div>

          <div class="flex justify-between items-center text-lg font-bold">
            <div class="w-1/3 text-right">{{ getTeamName(match.homeTeamId) }}</div>
            <div
              class="w-1/3 text-center py-1 rounded mx-4 text-2xl"
              :class="
                match.status === 'Finished' ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-900'
              "
            >
              {{ match.homeScore }} - {{ match.awayScore }}
            </div>
            <div class="w-1/3 text-left">{{ getTeamName(match.awayTeamId) }}</div>
          </div>

          <!-- Active Live Events -->
          <div
            v-if="match.status === 'Live' && String(liveMatchStore.activeMatchId) === String(match.id)"
            class="mt-6 pt-4 border-t border-gray-100"
          >
            <h4 class="text-sm font-bold text-gray-600 mb-3">Live Events</h4>
            <ul class="space-y-2">
              <li
                v-for="(event, index) in liveMatchStore.events"
                :key="index"
                class="text-sm flex items-center text-gray-700 bg-gray-50 p-2 rounded"
              >
                <span class="font-bold text-blue-600 mr-2">{{ event.minute }}'</span>
                <span
                  >⚽ Goal by <span class="font-bold">{{ getTeamName(event.teamId) }}</span></span
                >
              </li>
              <li v-if="liveMatchStore.events.length === 0" class="text-sm text-gray-400 italic">
                No events logged.
              </li>
            </ul>
          </div>

          <!-- Finished Match Events Dropdown -->
          <div v-if="match.status === 'Finished'" class="mt-4 border-t border-gray-100 pt-3">
            <button
              @click="toggleEvents(match.id)"
              class="w-full text-center text-sm font-semibold text-blue-600 hover:text-blue-800 transition"
            >
              {{ expandedMatches[match.id] ? 'Hide Events ▲' : 'Show Events ▼' }}
            </button>

            <div v-if="expandedMatches[match.id]" class="mt-4">
              <ul class="space-y-2">
                <li
                  v-for="(event, index) in match.events || []"
                  :key="index"
                  class="text-sm flex items-center text-gray-700 bg-gray-50 p-2 rounded"
                >
                  <span class="font-bold text-gray-800 mr-2">{{ event.minute }}'</span>
                  <span
                    >⚽ Goal by <span class="font-bold">{{ getTeamName(event.teamId) }}</span></span
                  >
                </li>
                <li
                  v-if="!(match.events && match.events.length)"
                  class="text-sm text-gray-400 italic text-center"
                >
                  No events were logged for this match.
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
