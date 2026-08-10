import { ref } from 'vue'
import { defineStore } from 'pinia'
import { useFixtureStore } from './fixtures'
import { MATCH_PHASES, MATCH_STATUS, EVENT_TYPES } from '../utils/constants'

export const useLiveMatchStore = defineStore('liveMatch', () => {
  const activeMatchId = ref(null)
  const currentMinute = ref(0)
  const events = ref([])
  const matchPhase = ref('')

  let timer = null
  let htTimeout = null

  const startMatch = async (matchId, lineup = {}) => {
    const fixtureStore = useFixtureStore()
    const match = fixtureStore.getMatchById(matchId)

    // Only bail out if the match is already running live (not just pre-selected)
    if (match && match.status === MATCH_STATUS.LIVE && activeMatchId.value === matchId) return

    if (timer) clearInterval(timer)
    if (htTimeout) clearTimeout(htTimeout)

    activeMatchId.value = matchId

    events.value = match.events || []

    if (match.status !== MATCH_STATUS.LIVE) {
      currentMinute.value = 0
      matchPhase.value = MATCH_PHASES.FIRST_HALF

      // Strip navigation properties before sending to backend to avoid EF Core conflicts
      const { homeTeam, awayTeam, ...matchData } = match
      await fixtureStore.updateFixture(matchId, {
        ...matchData,
        status: MATCH_STATUS.LIVE,
        homeScore: 0,
        awayScore: 0,
        events: [],
        ...(lineup.homeLineup !== undefined && { homeLineup: lineup.homeLineup }),
        ...(lineup.awayLineup !== undefined && { awayLineup: lineup.awayLineup }),
      })
    } else {
      if (events.value.length > 0) {
        currentMinute.value = events.value[events.value.length - 1].minute
      } else {
        currentMinute.value = 0
      }
      matchPhase.value = MATCH_PHASES.FIRST_HALF
    }

    runTimer()
  }

  const runTimer = () => {
    timer = setInterval(() => {
      if (matchPhase.value === MATCH_PHASES.FIRST_HALF) {
        currentMinute.value++
        if (currentMinute.value === 45) {
          clearInterval(timer)
          matchPhase.value = MATCH_PHASES.HALF_TIME
          htTimeout = setTimeout(() => {
            matchPhase.value = MATCH_PHASES.SECOND_HALF
            runTimer()
          }, 5000)
        }
      } else if (matchPhase.value === MATCH_PHASES.SECOND_HALF) {
        currentMinute.value++
        if (currentMinute.value >= 90) {
          concludeMatch()
        }
      }
    }, 222)
  }

  const concludeMatch = async () => {
    const fixtureStore = useFixtureStore()
    clearInterval(timer)
    clearTimeout(htTimeout)
    matchPhase.value = MATCH_PHASES.FULL_TIME

    if (activeMatchId.value) {
      const match = fixtureStore.getMatchById(activeMatchId.value)
      if (match) {
        const { homeTeam, awayTeam, ...matchData } = match
        await fixtureStore.updateFixture(activeMatchId.value, {
          ...matchData,
          status: MATCH_STATUS.FINISHED,
          events: events.value,
        })
      }
    }
  }

  const addGoal = (teamId) => {
    events.value.push({ type: EVENT_TYPES.GOAL, teamId: teamId, minute: currentMinute.value })
  }

  const cancelMatch = async () => {
    const fixtureStore = useFixtureStore()
    clearInterval(timer)
    clearTimeout(htTimeout)

    if (activeMatchId.value) {
      const match = fixtureStore.getMatchById(activeMatchId.value)
      if (match) {
        const { homeTeam, awayTeam, ...matchData } = match
        await fixtureStore.updateFixture(activeMatchId.value, {
          ...matchData,
          status: MATCH_STATUS.SCHEDULED,
          homeScore: 0,
          awayScore: 0,
          events: [],
        })
      }
    }
    activeMatchId.value = null
  }

  const clearMatch = () => {
    activeMatchId.value = null
    events.value = []
    currentMinute.value = 0
    matchPhase.value = ''
  }

  return {
    activeMatchId,
    currentMinute,
    events,
    matchPhase,
    startMatch,
    concludeMatch,
    addGoal,
    cancelMatch,
    clearMatch,
  }
})
