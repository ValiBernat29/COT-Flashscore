<script setup>
import { computed } from 'vue'
import { useTeamStore } from '../../stores/teams'
import { useFixtureStore } from '../../stores/fixtures'
import { useLiveMatchStore } from '../../stores/liveMatch'

const emit = defineEmits(['close'])
const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()
const liveMatchStore = useLiveMatchStore()

const getTeamName = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.name || 'Deleted Team'

const activeMatch = computed(() => {
  if (!liveMatchStore.activeMatchId) return null
  return fixtureStore.getMatchById(liveMatchStore.activeMatchId)
})

const handleCancelMatch = async () => {
  if (confirm('Are you sure you want to cancel? This resets the match to 0-0 and wipes events.')) {
    await liveMatchStore.cancelMatch()
    emit('close')
  }
}

const handleBack = () => {
  if (liveMatchStore.matchPhase === 'FT') {
    liveMatchStore.clearMatch()
  }
  emit('close')
}

const registerGoal = async (teamId) => {
  if (liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT') return

  liveMatchStore.addGoal(teamId)

  if (String(activeMatch.value.homeTeamId) === String(teamId)) {
    activeMatch.value.homeScore += 1
  } else {
    activeMatch.value.awayScore += 1
  }

  await fixtureStore.updateFixture(activeMatch.value.id, {
    ...activeMatch.value,
    homeScore: activeMatch.value.homeScore,
    awayScore: activeMatch.value.awayScore,
    events: liveMatchStore.events,
  })
}
</script>

<template>
  <div v-if="activeMatch" class="space-y-6">
    <div class="flex justify-between items-center pb-2 border-b border-slate-700">
      <button
        @click="handleBack"
        class="text-slate-400 hover:text-emerald-400 font-semibold transition flex items-center gap-2"
      >
        <span class="text-xl leading-none">←</span> Back to Match List
      </button>
      <span class="text-xs text-slate-500 italic">
        {{
          liveMatchStore.matchPhase === 'FT'
            ? 'Match has concluded'
            : 'Match continues running in the background'
        }}
      </span>
    </div>

    <div
      class="flex justify-between items-center bg-slate-900 p-8 rounded-lg border border-slate-700 relative overflow-hidden"
    >
      <div
        class="absolute top-0 left-1/2 -translate-x-1/2 bg-slate-800 px-6 py-2 rounded-b-lg border-b border-x border-slate-700 shadow-md flex items-center gap-3"
      >
        <span
          v-if="liveMatchStore.matchPhase === 'HT'"
          class="text-amber-400 font-black tracking-widest uppercase"
          >Half Time</span
        >
        <span
          v-else-if="liveMatchStore.matchPhase === 'FT'"
          class="text-slate-400 font-black tracking-widest uppercase"
          >Full Time</span
        >
        <span v-else class="text-emerald-400 font-bold text-xl animate-pulse"
          >{{ liveMatchStore.currentMinute }}'</span
        >
      </div>

      <div class="text-center w-1/3 mt-6">
        <h3 class="text-2xl font-bold text-white mb-4">
          {{ getTeamName(activeMatch.homeTeamId) }}
        </h3>
        <button
          @click="registerGoal(activeMatch.homeTeamId)"
          :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
          class="px-6 py-2 bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:text-slate-500 text-white rounded-full font-bold transition"
        >
          + Goal
        </button>
      </div>
      <div class="text-center w-1/3 mt-6">
        <div class="text-5xl font-black text-white tracking-widest mb-2">
          {{ activeMatch.homeScore }} - {{ activeMatch.awayScore }}
        </div>
        <div class="text-sm text-slate-400 mt-2">
          Events Logged: {{ liveMatchStore.events.length }}
        </div>
      </div>
      <div class="text-center w-1/3 mt-6">
        <h3 class="text-2xl font-bold text-white mb-4">
          {{ getTeamName(activeMatch.awayTeamId) }}
        </h3>
        <button
          @click="registerGoal(activeMatch.awayTeamId)"
          :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
          class="px-6 py-2 bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:text-slate-500 text-white rounded-full font-bold transition"
        >
          + Goal
        </button>
      </div>
    </div>

    <div class="flex justify-center gap-6 pt-4">
      <template v-if="liveMatchStore.matchPhase !== 'FT'">
        <button
          @click="liveMatchStore.concludeMatch()"
          class="px-8 py-2 bg-red-600 hover:bg-red-500 text-white font-bold rounded shadow-lg transition"
        >
          Conclude Match
        </button>
        <button
          @click="handleCancelMatch"
          class="px-8 py-2 border border-slate-600 text-slate-400 hover:bg-slate-700 hover:text-white rounded transition"
        >
          Cancel Match
        </button>
      </template>
      <template v-else>
        <button
          @click="handleBack"
          class="px-8 py-2 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded shadow-lg transition"
        >
          Close Match Controller
        </button>
      </template>
    </div>
  </div>
  <div v-else class="text-center py-12 text-slate-400">
    <p>No active match selected. Please select a match to manage.</p>
  </div>
</template>
