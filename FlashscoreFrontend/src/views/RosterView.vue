<script setup>
import { ref, watch, onMounted } from 'vue'
import { useTeamStore } from '../stores/teams'
import { usePlayerStore } from '../stores/players'

const teamStore = useTeamStore()
const playerStore = usePlayerStore()

// State
const selectedTeamId = ref('')
const newPlayerName = ref('')
const newPlayerNumber = ref('')
const newPlayerPosition = ref('MID')

// Fetch teams on load so the dropdown has data
onMounted(() => {
  if (!teamStore.teams?.length) {
    teamStore.fetchTeams()
  }
})

const selectedTeam = () => teamStore.teams.find((t) => String(t.id) === String(selectedTeamId.value))

// Watch for dropdown changes and fetch that specific team's players
watch(selectedTeamId, (newId) => {
  if (newId) {
    playerStore.fetchPlayersByTeam(newId)
  } else {
    playerStore.players = []
  }
})

const handleAddPlayer = async () => {
  if (!selectedTeamId.value || !newPlayerName.value || !newPlayerNumber.value) return

  await playerStore.addPlayer(
    selectedTeamId.value,
    newPlayerName.value,
    newPlayerNumber.value,
    newPlayerPosition.value,
  )

  // Reset form
  newPlayerName.value = ''
  newPlayerNumber.value = ''
  newPlayerPosition.value = 'MID'
}
</script>

<template>
  <div class="bg-white rounded-lg shadow p-6 max-w-4xl mx-auto">
    <h2 class="text-2xl font-bold mb-6 text-gray-800">Squad Management</h2>

    <div class="mb-8">
      <label class="block text-sm font-medium text-gray-700 mb-2">Select a Team</label>
      <select
        v-model="selectedTeamId"
        class="w-full border-gray-300 rounded-md shadow-sm p-3 border focus:ring-blue-500 focus:border-blue-500 text-gray-900 bg-white"
      >
        <option value="">-- Choose Team --</option>
        <option v-for="team in teamStore.teams" :key="team.id" :value="team.id">
          {{ team.name }}
        </option>
      </select>
    </div>

    <div v-if="selectedTeamId">
      <!-- Team logo banner -->
      <div class="flex items-center gap-4 py-4 mb-2">
        <img
          v-if="selectedTeam()?.logoUrl"
          :src="selectedTeam()?.logoUrl"
          :alt="selectedTeam()?.name"
          class="w-16 h-16 object-contain"
        />
        <div>
          <h3 class="text-xl font-black text-gray-900">{{ selectedTeam()?.name }}</h3>
          <p class="text-sm text-gray-500">{{ playerStore.players?.length ?? 0 }} players in squad</p>
        </div>
      </div>
      <hr class="my-4 border-gray-200" />

      <form @submit.prevent="handleAddPlayer" class="flex gap-4 mb-8 items-end">
        <div class="flex-1">
          <label class="block text-sm font-medium text-gray-700 mb-1">Player Name</label>
          <input
            v-model="newPlayerName"
            type="text"
            required
            class="w-full border border-gray-300 rounded p-2 text-gray-900 bg-white"
            placeholder="e.g. Lionel Messi"
          />
        </div>
        <div class="w-24">
          <label class="block text-sm font-medium text-gray-700 mb-1">Number</label>
          <input
            v-model="newPlayerNumber"
            type="number"
            required
            min="1"
            max="99"
            class="w-full border border-gray-300 rounded p-2 text-gray-900 bg-white"
          />
        </div>
        <div class="w-32">
          <label class="block text-sm font-medium text-gray-700 mb-1">Position</label>
          <select
            v-model="newPlayerPosition"
            class="w-full border border-gray-300 rounded p-2 text-gray-900 bg-white"
          >
            <option value="GK">GK</option>
            <option value="DEF">DEF</option>
            <option value="MID">MID</option>
            <option value="FWD">FWD</option>
          </select>
        </div>
        <button
          type="submit"
          class="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700 font-semibold h-10.5"
        >
          Add
        </button>
      </form>

      <div
        v-if="playerStore.players?.length > 0"
        class="overflow-hidden border border-gray-200 rounded-lg"
      >
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Photo</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">No.</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Position</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="player in playerStore.players" :key="player.id" class="hover:bg-gray-50">
              <td class="px-4 py-3">
                <img
                  v-if="player.photoUrl"
                  :src="player.photoUrl"
                  :alt="player.name"
                  class="w-10 h-10 rounded-full object-cover bg-gray-200"
                />
                <div v-else class="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-sm font-bold text-gray-500">
                  {{ player.number }}
                </div>
              </td>
              <td class="px-4 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                #{{ player.number }}
              </td>
              <td class="px-4 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {{ player.name }}
              </td>
              <td class="px-4 py-4 whitespace-nowrap text-sm text-gray-500">
                <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-100 text-gray-800">
                  {{ player.position }}
                </span>
              </td>
              <td class="px-4 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button
                  @click="playerStore.deletePlayer(player.id)"
                  class="text-red-600 hover:text-red-900 font-semibold transition"
                >
                  Remove
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else class="text-center py-8 text-gray-500 italic">
        No players found for this team. Add some above!
      </div>
    </div>
  </div>
</template>
