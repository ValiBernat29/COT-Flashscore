<script setup>
import { ref, computed, onMounted } from 'vue'
import { useTeamStore } from '../../stores/teams'
import { useFixtureStore } from '../../stores/fixtures'
import { useLiveMatchStore } from '../../stores/liveMatch'
import { MATCH_STATUS, EVENT_TYPES } from '../../utils/constants' 
import { usePlayerStore } from '../../stores/players' 

const emit = defineEmits(['close'])
const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()
const liveMatchStore = useLiveMatchStore()
const playerStore = usePlayerStore()

const selectedHomeLineup = ref([])
const selectedAwayLineup = ref([])

const homeCardPlayerId = ref('')
const awayCardPlayerId = ref('')
const cardError = ref('')

const activeMatch = computed(() => {
  if (!liveMatchStore.activeMatchId) return null
    return fixtureStore.getMatchById(Number(liveMatchStore.activeMatchId)) 
      || fixtureStore.getMatchById(String(liveMatchStore.activeMatchId))
})

const homeTeamRoster = ref([])
const awayTeamRoster = ref([])

onMounted(async () => {
  if (!activeMatch.value) return

  try {
    const homeRes = await fetch(`http://localhost:5198/api/players/team/${activeMatch.value.homeTeamId}`)
    homeTeamRoster.value = await homeRes.json()

    const awayRes = await fetch(`http://localhost:5198/api/players/team/${activeMatch.value.awayTeamId}`)
    awayTeamRoster.value = await awayRes.json()
  } catch (error) {
    console.error('Failed to load match rosters:', error)
  }
})

const isLineupReady = computed(() => {
  return selectedHomeLineup.value.length === 11 && selectedAwayLineup.value.length === 11
})

const startMatch = async () => {
  await liveMatchStore.startMatch(activeMatch.value.id, {
    homeLineup: selectedHomeLineup.value,
    awayLineup: selectedAwayLineup.value,
  })
}

const getTeamName = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.name || 'Deleted Team'

const getTeamLogo = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.logoUrl || ''

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

  const { homeTeam, awayTeam, ...matchData } = activeMatch.value
  await fixtureStore.updateFixture(activeMatch.value.id, {
    ...matchData,
    homeScore: activeMatch.value.homeScore,
    awayScore: activeMatch.value.awayScore,
    events: liveMatchStore.events,
  })
}

const isSentOff = (playerId) => {
  const playerEvents = liveMatchStore.events.filter(
    (e) => String(e.playerId) === String(playerId)
  )
  const yellowCount = playerEvents.filter((e) => e.type === EVENT_TYPES.YELLOW_CARD).length
  const hasRed = playerEvents.some((e) => e.type === EVENT_TYPES.RED_CARD)
  return hasRed || yellowCount >= 2
}

const registerCard = async (teamId, playerId, type) => {
  if (liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT') return
  if (!playerId) return

  const playerEvents = liveMatchStore.events.filter(
    (e) => String(e.playerId) === String(playerId)
  )
  const yellowCount = playerEvents.filter((e) => e.type === EVENT_TYPES.YELLOW_CARD).length
  const hasRed = playerEvents.some((e) => e.type === EVENT_TYPES.RED_CARD)

  // Already sent off — nothing more can be issued
  if (hasRed || yellowCount >= 2) {
    cardError.value = 'This player has already been sent off.'
    setTimeout(() => { cardError.value = '' }, 2500)
    return
  }

  await liveMatchStore.addCard(teamId, playerId, type)

  // 2nd yellow automatically triggers a red card
  if (type === EVENT_TYPES.YELLOW_CARD && yellowCount === 1) {
    await liveMatchStore.addCard(teamId, playerId, EVENT_TYPES.RED_CARD)
  }
}

const homeLineupPlayers = computed(() =>
  activeMatch.value
    ? homeTeamRoster.value.filter(p => activeMatch.value.homeLineup?.includes(p.id))
    : []
)

const awayLineupPlayers = computed(() =>
  activeMatch.value
    ? awayTeamRoster.value.filter(p => activeMatch.value.awayLineup?.includes(p.id))
    : []
)
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
            : activeMatch.status === MATCH_STATUS.SCHEDULED 
            ? 'Pre-match setup' 
            : 'Match continues running in the background'
        }}
      </span>
    </div>

    <div v-if="activeMatch.status === MATCH_STATUS.SCHEDULED" class="bg-slate-800 p-6 rounded-lg mt-6">
      <h2 class="text-2xl font-bold text-white mb-4">Set Starting Lineups</h2>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
        <div>
          <h3 class="text-xl text-blue-400 font-bold mb-3">
            <img v-if="getTeamLogo(activeMatch.homeTeamId)" :src="getTeamLogo(activeMatch.homeTeamId)" class="inline w-6 h-6 object-contain mr-2" />
            {{ getTeamName(activeMatch.homeTeamId) }} ({{ selectedHomeLineup.length }}/11)
          </h3>
          <div class="space-y-2 max-h-96 overflow-y-auto pr-2">
            <label 
              v-for="player in homeTeamRoster" 
              :key="player.id"
              class="flex items-center gap-3 p-2 rounded hover:bg-slate-700 cursor-pointer transition border border-slate-700"
              :class="{'bg-blue-900/30 border-blue-500': selectedHomeLineup.includes(player.id)}"
            >
              <input type="checkbox" :value="player.id" v-model="selectedHomeLineup" class="w-5 h-5 accent-blue-500">
              <img
                v-if="player.photoUrl"
                :src="player.photoUrl"
                :alt="player.name"
                class="w-8 h-8 rounded-full object-cover bg-slate-600 flex-shrink-0"
              />
              <div v-else class="w-8 h-8 rounded-full bg-slate-600 flex items-center justify-center flex-shrink-0 text-xs text-slate-400">#{{ player.number }}</div>
              <span class="font-bold text-white flex-1">{{ player.name }}</span>
              <span class="text-xs bg-slate-600 px-2 py-1 rounded text-slate-300">{{ player.position }}</span>
            </label>
            <div v-if="homeTeamRoster.length === 0" class="text-slate-500 italic p-4 text-center border border-dashed border-slate-700 rounded">
              No players found for this team. Add players in the Teams menu first!
            </div>
          </div>
        </div>

        <div>
          <h3 class="text-xl text-red-400 font-bold mb-3">
            <img v-if="getTeamLogo(activeMatch.awayTeamId)" :src="getTeamLogo(activeMatch.awayTeamId)" class="inline w-6 h-6 object-contain mr-2" />
            {{ getTeamName(activeMatch.awayTeamId) }} ({{ selectedAwayLineup.length }}/11)
          </h3>
          <div class="space-y-2 max-h-96 overflow-y-auto pr-2">
            <label 
              v-for="player in awayTeamRoster" 
              :key="player.id"
              class="flex items-center gap-3 p-2 rounded hover:bg-slate-700 cursor-pointer transition border border-slate-700"
              :class="{'bg-red-900/30 border-red-500': selectedAwayLineup.includes(player.id)}"
            >
              <input type="checkbox" :value="player.id" v-model="selectedAwayLineup" class="w-5 h-5 accent-red-500">
              <img
                v-if="player.photoUrl"
                :src="player.photoUrl"
                :alt="player.name"
                class="w-8 h-8 rounded-full object-cover bg-slate-600 flex-shrink-0"
              />
              <div v-else class="w-8 h-8 rounded-full bg-slate-600 flex items-center justify-center flex-shrink-0 text-xs text-slate-400">#{{ player.number }}</div>
              <span class="font-bold text-white flex-1">{{ player.name }}</span>
              <span class="text-xs bg-slate-600 px-2 py-1 rounded text-slate-300">{{ player.position }}</span>
            </label>
            <div v-if="awayTeamRoster.length === 0" class="text-slate-500 italic p-4 text-center border border-dashed border-slate-700 rounded">
              No players found for this team. Add players in the Teams menu first!
            </div>
          </div>
        </div>
      </div>

      <div class="mt-8 flex flex-col items-center gap-2">
        <button 
          @click="startMatch" 
          class="px-8 py-3 bg-green-600 hover:bg-green-500 text-white rounded-full font-bold text-lg transition shadow-lg shadow-green-500/30"
        >
          Start Match
        </button>
        <span v-if="!isLineupReady" class="text-amber-500 text-sm font-semibold">
          Warning: You have not selected exactly 11 players for both teams!
        </span>
      </div>
    </div>

    <div v-else>
      <div class="flex justify-between items-center bg-slate-900 p-8 rounded-lg border border-slate-700 relative overflow-hidden">
        <div class="absolute top-0 left-1/2 -translate-x-1/2 bg-slate-800 px-6 py-2 rounded-b-lg border-b border-x border-slate-700 shadow-md flex items-center gap-3">
          <span v-if="liveMatchStore.matchPhase === 'HT'" class="text-amber-400 font-black tracking-widest uppercase">Half Time</span>
          <span v-else-if="liveMatchStore.matchPhase === 'FT'" class="text-slate-400 font-black tracking-widest uppercase">Full Time</span>
          <span v-else class="text-emerald-400 font-bold text-xl animate-pulse">{{ liveMatchStore.currentMinute }}'</span>
        </div>

        <div class="text-center w-1/3 mt-6">
          <div class="flex flex-col items-center gap-2 mb-4">
            <img v-if="getTeamLogo(activeMatch.homeTeamId)" :src="getTeamLogo(activeMatch.homeTeamId)" class="w-12 h-12 object-contain" />
            <h3 class="text-2xl font-bold text-white">{{ getTeamName(activeMatch.homeTeamId) }}</h3>
          </div>
          <button
            @click="registerGoal(activeMatch.homeTeamId)"
            :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:text-slate-500 text-white rounded-full font-bold transition"
          >
            + Goal
          </button>
          <div class="mt-3 flex flex-col gap-2 items-center">
            <select
              v-model="homeCardPlayerId"
              :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
              class="w-full max-w-[180px] bg-slate-800 border border-slate-600 text-slate-200 text-sm rounded px-2 py-1 disabled:opacity-40"
            >
              <option disabled value="">Select player…</option>
              <option v-for="p in homeLineupPlayers" :key="p.id" :value="p.id">
                {{ isSentOff(p.id) ? '🚨 ' : '' }}#{{ p.number }} {{ p.name }}{{ isSentOff(p.id) ? ' (off)' : '' }}
              </option>
            </select>
            <p v-if="cardError" class="text-xs text-red-400 font-semibold">{{ cardError }}</p>
            <div class="flex gap-2">
              <button
                @click="registerCard(activeMatch.homeTeamId, homeCardPlayerId, EVENT_TYPES.YELLOW_CARD)"
                :disabled="!homeCardPlayerId || liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
                class="px-3 py-1 bg-yellow-500 hover:bg-yellow-400 disabled:bg-slate-700 disabled:text-slate-500 text-slate-900 font-bold text-sm rounded-full transition"
              >🟨 Yellow</button>
              <button
                @click="registerCard(activeMatch.homeTeamId, homeCardPlayerId, EVENT_TYPES.RED_CARD)"
                :disabled="!homeCardPlayerId || liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
                class="px-3 py-1 bg-red-600 hover:bg-red-500 disabled:bg-slate-700 disabled:text-slate-500 text-white font-bold text-sm rounded-full transition"
              >🟥 Red</button>
            </div>
          </div>
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
          <div class="flex flex-col items-center gap-2 mb-4">
            <img v-if="getTeamLogo(activeMatch.awayTeamId)" :src="getTeamLogo(activeMatch.awayTeamId)" class="w-12 h-12 object-contain" />
            <h3 class="text-2xl font-bold text-white">{{ getTeamName(activeMatch.awayTeamId) }}</h3>
          </div>
          <button
            @click="registerGoal(activeMatch.awayTeamId)"
            :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:text-slate-500 text-white rounded-full font-bold transition"
          >
            + Goal
          </button>
          <div class="mt-3 flex flex-col gap-2 items-center">
            <select
              v-model="awayCardPlayerId"
              :disabled="liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
              class="w-full max-w-[180px] bg-slate-800 border border-slate-600 text-slate-200 text-sm rounded px-2 py-1 disabled:opacity-40"
            >
              <option disabled value="">Select player…</option>
              <option v-for="p in awayLineupPlayers" :key="p.id" :value="p.id">
                {{ isSentOff(p.id) ? '🚨 ' : '' }}#{{ p.number }} {{ p.name }}{{ isSentOff(p.id) ? ' (off)' : '' }}
              </option>
            </select>
            <p v-if="cardError" class="text-xs text-red-400 font-semibold">{{ cardError }}</p>
            <div class="flex gap-2">
              <button
                @click="registerCard(activeMatch.awayTeamId, awayCardPlayerId, EVENT_TYPES.YELLOW_CARD)"
                :disabled="!awayCardPlayerId || liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
                class="px-3 py-1 bg-yellow-500 hover:bg-yellow-400 disabled:bg-slate-700 disabled:text-slate-500 text-slate-900 font-bold text-sm rounded-full transition"
              >🟨 Yellow</button>
              <button
                @click="registerCard(activeMatch.awayTeamId, awayCardPlayerId, EVENT_TYPES.RED_CARD)"
                :disabled="!awayCardPlayerId || liveMatchStore.matchPhase === 'HT' || liveMatchStore.matchPhase === 'FT'"
                class="px-3 py-1 bg-red-600 hover:bg-red-500 disabled:bg-slate-700 disabled:text-slate-500 text-white font-bold text-sm rounded-full transition"
              >🟥 Red</button>
            </div>
          </div>
        </div>
      </div>

      <div class="flex justify-center gap-6 pt-4">
        <template v-if="liveMatchStore.matchPhase !== 'FT'">
          <button @click="liveMatchStore.concludeMatch()" class="px-8 py-2 bg-red-600 hover:bg-red-500 text-white font-bold rounded shadow-lg transition">
            Conclude Match
          </button>
          <button @click="handleCancelMatch" class="px-8 py-2 border border-slate-600 text-slate-400 hover:bg-slate-700 hover:text-white rounded transition">
            Cancel Match
          </button>
        </template>
        <template v-else>
          <button @click="handleBack" class="px-8 py-2 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded shadow-lg transition">
            Close Match Controller
          </button>
        </template>
      </div>
    </div>

  </div>
  
 <div v-else class="text-center py-12 text-slate-400">
    <p class="mb-4">No active match selected, or the loaded match was deleted.</p>
    
    <button 
      v-if="liveMatchStore.activeMatchId" 
      @click="liveMatchStore.clearMatch()"
      class="px-6 py-2 bg-red-600 hover:bg-red-500 text-white rounded-full font-bold shadow-lg transition"
    >
      Force Clear Stuck Match
    </button>
  </div>
</template>