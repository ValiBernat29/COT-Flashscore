<script setup>
import { useFixtureStore } from '../stores/fixtures'
import { useTeamStore } from '../stores/teams'
import { useLiveMatchStore } from '../stores/liveMatch'
import { RouterLink } from 'vue-router'

const fixtureStore = useFixtureStore()
const teamStore = useTeamStore()
const liveMatchStore = useLiveMatchStore()

const getTeamName = (id) => teamStore.teams.find((t) => String(t.id) === String(id))?.name
</script>

<template>
  <div class="space-y-8">
    <h2 class="text-3xl font-bold text-gray-800">Matchday Schedule</h2>
    <div v-if="fixtureStore.matchesByMatchday.length === 0" class="text-gray-500 italic">
      No matches scheduled yet.
    </div>

    <div v-for="group in fixtureStore.matchesByMatchday" :key="group.matchday" class="space-y-3">
      <div class="border-b-2 border-blue-600 pb-2 mb-4">
        <h3 class="text-xl font-bold text-gray-700 uppercase tracking-wide">
          Matchday {{ group.matchday }}
        </h3>
      </div>

      <RouterLink
        v-for="match in group.matches"
        :key="match.id"
        :to="{ name: 'match-detail', params: { id: match.id } }"
        class="block bg-white rounded-lg shadow border-l-4 p-4 hover:shadow-md hover:-translate-y-0.5 transition-all duration-150 group"
        :class="{
          'border-red-500': match.status === 'Live',
          'border-gray-800': match.status === 'Finished',
          'border-gray-300': match.status === 'Scheduled',
        }"
      >
        <div class="flex items-center justify-between mb-3">
          <div class="flex items-center gap-2">
            <span
              v-if="match.status === 'Live'"
              class="flex items-center gap-1.5 text-xs font-bold text-red-500 uppercase tracking-wider"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-red-500 animate-pulse inline-block"></span>
              Live
            </span>
            <span
              v-else-if="match.status === 'Finished'"
              class="text-xs font-bold text-gray-700 uppercase tracking-wider"
            >FT</span>
            <span v-else class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Scheduled</span>

            <span
              v-if="match.status === 'Live' && String(liveMatchStore.activeMatchId) === String(match.id)"
              class="text-xs font-bold px-2 py-0.5 rounded bg-gray-100"
              :class="liveMatchStore.matchPhase === 'HT' ? 'text-amber-500' : 'text-emerald-600'"
            >
              {{ liveMatchStore.matchPhase === 'HT' ? 'HT' : liveMatchStore.currentMinute + "'" }}
            </span>
          </div>

          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="w-4 h-4 text-gray-300 group-hover:text-blue-500 transition-colors"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd"/>
          </svg>
        </div>

        <div class="flex items-center justify-between text-lg font-bold">
          <div class="w-2/5 text-right truncate">{{ getTeamName(match.homeTeamId) }}</div>
          <div
            class="w-1/5 text-center py-1 rounded mx-3 text-xl font-black"
            :class="match.status === 'Finished' ? 'bg-gray-800 text-white' : 'bg-gray-100 text-gray-900'"
          >
            {{ match.homeScore }} – {{ match.awayScore }}
          </div>
          <div class="w-2/5 text-left truncate">{{ getTeamName(match.awayTeamId) }}</div>
        </div>
      </RouterLink>
    </div>
  </div>
</template>
