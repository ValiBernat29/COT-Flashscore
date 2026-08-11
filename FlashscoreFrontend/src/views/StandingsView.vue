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
    played: 0,
    won: 0,
    drawn: 0,
    lost: 0,
    gf: 0,
    ga: 0,
    gd: 0,
    points: 0,
  }))

  const safeFixtures = fixtureStore.fixtures || []

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
    } else if (match.homeScore < match.awayScore) {
      away.won++
      away.points += 3
      home.lost++
    } else {
      home.drawn++
      home.points += 1
      away.drawn++
      away.points += 1
    }
  })

  // Sort by Points, then Goal Difference, then Goals For
  return table.sort((a, b) => b.points - a.points || (b.gf - b.ga) - (a.gf - a.ga) || b.gf - a.gf)
})
</script>

<template>
  <div class="bg-white rounded-lg shadow p-6">
    <h2 class="text-2xl font-bold mb-4 text-gray-800">League Table (Live)</h2>

    <div class="overflow-x-auto">
      <table class="w-full text-left border-collapse whitespace-nowrap">
        <thead>
          <tr class="bg-gray-100 text-gray-700 uppercase text-sm border-b-2 border-gray-200">
            <th class="p-3">Pos</th>
            <th class="p-3">Team</th>
            <th class="p-3 text-center">P</th>
            <th class="p-3 text-center">W</th>
            <th class="p-3 text-center">D</th>
            <th class="p-3 text-center">L</th>
            <th class="p-3 text-center">GF</th>
            <th class="p-3 text-center">GA</th>
            <th class="p-3 text-center text-blue-600">Pts</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(team, index) in standings"
            :key="team.id"
            class="border-b hover:bg-gray-50 transition"
          >
            <td class="p-3 font-semibold text-gray-500">{{ index + 1 }}</td>
            <td class="p-3 font-bold text-gray-900">{{ team.name }}</td>
            <td class="p-3 text-center">{{ team.played }}</td>
            <td class="p-3 text-center">{{ team.won }}</td>
            <td class="p-3 text-center">{{ team.drawn }}</td>
            <td class="p-3 text-center">{{ team.lost }}</td>
            <td class="p-3 text-center">{{ team.gf }}</td>
            <td class="p-3 text-center">{{ team.ga }}</td>
            <td class="p-3 text-center font-bold text-blue-600 text-lg">{{ team.points }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
