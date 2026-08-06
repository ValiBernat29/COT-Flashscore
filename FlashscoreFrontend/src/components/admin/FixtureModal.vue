<script setup>
import { ref } from 'vue'
import { useTeamStore } from '../../stores/teams'
import { useFixtureStore } from '../../stores/fixtures'

const emit = defineEmits(['close'])
const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()

const newMatchday = ref(1)
const newHomeTeam = ref('')
const newAwayTeam = ref('')

const getTeamName = (id) =>
  teamStore.teams.find((t) => String(t.id) === String(id))?.name || 'Deleted Team'

const handleAddFixture = () => {
  if (!newHomeTeam.value || !newAwayTeam.value || newHomeTeam.value === newAwayTeam.value) {
    alert('Please select two distinct teams.')
    return
  }
  fixtureStore.addFixture(newMatchday.value, newHomeTeam.value, newAwayTeam.value)
  newHomeTeam.value = ''
  newAwayTeam.value = ''
}

const handleDeleteFixture = (id) => {
  if (confirm('Delete this fixture?')) {
    window.dispatchEvent(new PointerEvent('pointercancel', { bubbles: true, pointerId: 1 }))
    window.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }))
    fixtureStore.deleteFixture(id)
  }
}
</script>

<template>
  <div class="fixed inset-0 bg-black/70 flex items-center justify-center z-50 px-4">
    <div
      class="bg-slate-800 rounded-lg shadow-2xl w-full max-w-2xl border border-slate-600 overflow-hidden"
    >
      <div class="p-4 border-b border-slate-700 flex justify-between items-center bg-slate-900">
        <h3 class="text-lg font-bold text-white">Manage Fixtures</h3>
        <button @click="emit('close')" class="text-slate-400 hover:text-white text-xl leading-none">
          &times;
        </button>
      </div>
      <div class="p-6 space-y-6">
        <form @submit.prevent="handleAddFixture" class="flex flex-col sm:flex-row gap-3">
          <input
            v-model="newMatchday"
            type="number"
            min="1"
            placeholder="Day"
            class="w-20 bg-slate-900 border border-slate-600 rounded px-4 py-2 text-white"
          />
          <select
            v-model="newHomeTeam"
            class="flex-1 bg-slate-900 border border-slate-600 rounded px-4 py-2 text-white"
          >
            <option disabled value="">Home Team...</option>
            <option v-for="team in teamStore.teams" :key="team.id" :value="team.id">
              {{ team.name }}
            </option>
          </select>
          <span class="text-white self-center font-bold">VS</span>
          <select
            v-model="newAwayTeam"
            class="flex-1 bg-slate-900 border border-slate-600 rounded px-4 py-2 text-white"
          >
            <option disabled value="">Away Team...</option>
            <option v-for="team in teamStore.teams" :key="team.id" :value="team.id">
              {{ team.name }}
            </option>
          </select>
          <button
            type="submit"
            class="px-6 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded font-bold"
          >
            Add
          </button>
        </form>
        <div class="max-h-60 overflow-y-auto space-y-4 pr-2">
          <div v-for="group in fixtureStore.matchesByMatchday" :key="group.matchday">
            <h4
              class="text-slate-400 font-bold mb-2 border-b border-slate-600 pb-1 text-sm uppercase"
            >
              Matchday {{ group.matchday }}
            </h4>
            <div class="space-y-2">
              <div
                v-for="match in group.matches"
                :key="match.id"
                class="flex justify-between items-center bg-slate-700 p-3 rounded border border-slate-600"
              >
                <span class="text-white font-medium"
                  >{{ getTeamName(match.homeTeamId) }} vs {{ getTeamName(match.awayTeamId) }}</span
                >
                <button
                  @click="handleDeleteFixture(match.id)"
                  class="text-red-400 hover:text-red-300 font-semibold text-sm"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
