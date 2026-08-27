<script setup>
import { computed } from 'vue'
import { useTeamStore } from '../stores/teams'
import { useFixtureStore } from '../stores/fixtures'

const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()

// Stats are recalculated live from fixture results rather than reading stored team
// stats, so the table always reflects the current score even mid-game
const standings = computed(() => {
  // Guard clause: Return empty list if teams or fixtures aren't loaded yet
  if (!teamStore.teams?.length) return []

  const table = teamStore.teams.map((team) => ({
    id: team.id,
    name: team.name,
    logoUrl: team.logoUrl,
    played: 0,
    won: 0,
    drawn: 0,
    lost: 0,
    gf: 0,
    ga: 0,
    gd: 0,
    points: 0,
    form: [], // last 5 results: 'W', 'D', 'L'
  }))

  const safeFixtures = fixtureStore.fixtures || []

  // Collect finished results per team to compute form
  const finishedByTeam = {}

  safeFixtures.forEach((match) => {
    // Calculate finished AND live games so the table updates in real-time
    if (match.status !== 'Finished' && match.status !== 'Live') return

    const home = table.find((t) => String(t.id) === String(match.homeTeamId))
    const away = table.find((t) => String(t.id) === String(match.awayTeamId))

    if (!home || !away) return

    home.played++
    away.played++
    home.gf += match.homeScore
    home.ga += match.awayScore
    away.gf += match.awayScore
    away.ga += match.homeScore

    if (match.homeScore > match.awayScore) {
      home.won++
      home.points += 3
      away.lost++
      if (match.status === 'Finished') {
        ;(finishedByTeam[home.id] = finishedByTeam[home.id] || []).push({ pts: 3, matchId: match.id })
        ;(finishedByTeam[away.id] = finishedByTeam[away.id] || []).push({ pts: 0, matchId: match.id })
      }
    } else if (match.homeScore < match.awayScore) {
      away.won++
      away.points += 3
      home.lost++
      if (match.status === 'Finished') {
        ;(finishedByTeam[away.id] = finishedByTeam[away.id] || []).push({ pts: 3, matchId: match.id })
        ;(finishedByTeam[home.id] = finishedByTeam[home.id] || []).push({ pts: 0, matchId: match.id })
      }
    } else {
      home.drawn++
      home.points += 1
      away.drawn++
      away.points += 1
      if (match.status === 'Finished') {
        ;(finishedByTeam[home.id] = finishedByTeam[home.id] || []).push({ pts: 1, matchId: match.id })
        ;(finishedByTeam[away.id] = finishedByTeam[away.id] || []).push({ pts: 1, matchId: match.id })
      }
    }
  })

  // Attach last-5 form
  table.forEach((team) => {
    const results = (finishedByTeam[team.id] || []).slice(-5)
    team.form = results.map((r) => (r.pts === 3 ? 'W' : r.pts === 1 ? 'D' : 'L'))
    team.gd = team.gf - team.ga
  })

  // Sort by Points, then Goal Difference, then Goals For
  return table.sort((a, b) => b.points - a.points || b.gd - a.gd || b.gf - a.gf)
})

// Zone helpers
const totalTeams = computed(() => standings.value.length)

const getZone = (index) => {
  const n = totalTeams.value
  if (n === 0) return 'normal'
  if (index < 2) return 'champions'
  if (index === 2) return 'europa'
  if (index >= n - 3) return 'relegation'
  return 'normal'
}

const zoneLabel = (index) => {
  const z = getZone(index)
  if (z === 'champions') return 'UCL'
  if (z === 'europa') return 'UEL'
  if (z === 'relegation') return 'REL'
  return ''
}

const gdDisplay = (gd) => (gd > 0 ? `+${gd}` : `${gd}`)
</script>

<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex items-center gap-3">
      <img src="https://media.api-sports.io/football/leagues/283.png" alt="Liga 1" class="w-10 h-10 object-contain" @error="$event.target.style.display='none'" />
      <div>
        <h2 class="text-2xl font-black text-gray-900 leading-tight">Liga 1 Romania</h2>
        <p class="text-sm text-gray-500 font-medium">Season 2024/25 · Live Table</p>
      </div>
    </div>

    <!-- Legend -->
    <div class="flex flex-wrap items-center gap-4 text-xs font-semibold text-gray-600">
      <div class="flex items-center gap-1.5">
        <span class="w-3 h-3 rounded-sm bg-blue-600 inline-block"></span> Champions League
      </div>
      <div class="flex items-center gap-1.5">
        <span class="w-3 h-3 rounded-sm bg-orange-400 inline-block"></span> Europa League
      </div>
      <div class="flex items-center gap-1.5">
        <span class="w-3 h-3 rounded-sm bg-red-500 inline-block"></span> Relegation
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="standings.length === 0" class="bg-white rounded-xl shadow border border-gray-200 p-12 text-center">
      <span class="text-4xl">⚽</span>
      <p class="mt-3 text-gray-500 font-medium">No data yet — sync teams first or wait for fixtures to be played.</p>
    </div>

    <!-- Table -->
    <div v-else class="bg-white rounded-xl shadow-md border border-gray-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse whitespace-nowrap text-sm">
          <thead>
            <tr class="bg-gray-50 border-b-2 border-gray-200 text-gray-500 uppercase text-xs tracking-wide font-semibold">
              <th class="px-4 py-3 w-8 text-center">#</th>
              <th class="px-4 py-3">Club</th>
              <th class="px-3 py-3 text-center" title="Played">P</th>
              <th class="px-3 py-3 text-center" title="Won">W</th>
              <th class="px-3 py-3 text-center" title="Drawn">D</th>
              <th class="px-3 py-3 text-center" title="Lost">L</th>
              <th class="px-3 py-3 text-center" title="Goals For">GF</th>
              <th class="px-3 py-3 text-center" title="Goals Against">GA</th>
              <th class="px-3 py-3 text-center" title="Goal Difference">GD</th>
              <th class="px-3 py-3 text-center" title="Form (last 5)">Form</th>
              <th class="px-4 py-3 text-center font-bold text-blue-600" title="Points">Pts</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(team, index) in standings"
              :key="team.id"
              class="border-b border-gray-100 hover:bg-blue-50/40 transition-colors duration-100 group"
              :class="{
                'border-l-4 border-l-blue-600': getZone(index) === 'champions',
                'border-l-4 border-l-orange-400': getZone(index) === 'europa',
                'border-l-4 border-l-red-500': getZone(index) === 'relegation',
                'border-l-4 border-l-transparent': getZone(index) === 'normal',
              }"
            >
              <!-- Position -->
              <td class="px-4 py-3 text-center">
                <span
                  class="inline-flex items-center justify-center w-6 h-6 rounded-full text-xs font-bold"
                  :class="{
                    'bg-blue-100 text-blue-700': getZone(index) === 'champions',
                    'bg-orange-100 text-orange-600': getZone(index) === 'europa',
                    'bg-red-100 text-red-600': getZone(index) === 'relegation',
                    'bg-gray-100 text-gray-600': getZone(index) === 'normal',
                  }"
                >{{ index + 1 }}</span>
              </td>

              <!-- Club name + logo -->
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="w-7 h-7 flex-shrink-0 flex items-center justify-center">
                    <img
                      v-if="team.logoUrl"
                      :src="team.logoUrl"
                      :alt="team.name"
                      class="w-7 h-7 object-contain drop-shadow-sm"
                    />
                    <span v-else class="text-lg">⚽</span>
                  </div>
                  <span class="font-semibold text-gray-900 group-hover:text-blue-700 transition-colors">{{ team.name }}</span>
                  <span
                    v-if="zoneLabel(index)"
                    class="text-[10px] font-bold px-1.5 py-0.5 rounded"
                    :class="{
                      'bg-blue-100 text-blue-700': getZone(index) === 'champions',
                      'bg-orange-100 text-orange-600': getZone(index) === 'europa',
                      'bg-red-100 text-red-600': getZone(index) === 'relegation',
                    }"
                  >{{ zoneLabel(index) }}</span>
                </div>
              </td>

              <!-- Stats -->
              <td class="px-3 py-3 text-center text-gray-600">{{ team.played }}</td>
              <td class="px-3 py-3 text-center font-semibold text-emerald-600">{{ team.won }}</td>
              <td class="px-3 py-3 text-center text-gray-500">{{ team.drawn }}</td>
              <td class="px-3 py-3 text-center font-semibold text-red-500">{{ team.lost }}</td>
              <td class="px-3 py-3 text-center text-gray-600">{{ team.gf }}</td>
              <td class="px-3 py-3 text-center text-gray-600">{{ team.ga }}</td>

              <!-- GD with color -->
              <td class="px-3 py-3 text-center font-semibold"
                :class="{
                  'text-emerald-600': team.gd > 0,
                  'text-red-500': team.gd < 0,
                  'text-gray-500': team.gd === 0
                }"
              >{{ gdDisplay(team.gd) }}</td>

              <!-- Form -->
              <td class="px-3 py-3">
                <div class="flex items-center justify-center gap-0.5">
                  <span
                    v-if="team.form.length === 0"
                    class="text-xs text-gray-400 italic"
                  >—</span>
                  <span
                    v-for="(result, i) in team.form"
                    :key="i"
                    class="inline-flex items-center justify-center w-5 h-5 rounded text-[10px] font-black text-white"
                    :class="{
                      'bg-emerald-500': result === 'W',
                      'bg-amber-400': result === 'D',
                      'bg-red-500': result === 'L',
                    }"
                  >{{ result }}</span>
                </div>
              </td>

              <!-- Points -->
              <td class="px-4 py-3 text-center">
                <span class="inline-block min-w-[2rem] text-center font-black text-base text-blue-600 bg-blue-50 rounded-md px-2 py-0.5 group-hover:bg-blue-600 group-hover:text-white transition-all duration-150">
                  {{ team.points }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Footer note -->
    <p class="text-xs text-gray-400 text-right">
      ⚡ Table recalculated live from match results — updates automatically during live games.
    </p>
  </div>
</template>
