<script setup>
import { ref } from 'vue'
import { useTeamStore } from '../../stores/teams'

const emit = defineEmits(['close'])
const teamStore = useTeamStore()
const newTeamName = ref('')

const handleAddTeam = () => {
  if (newTeamName.value.trim() === '') return
  teamStore.addTeam(newTeamName.value)
  newTeamName.value = ''
}

const handleDeleteTeam = (id) => {
  if (confirm('Delete this team? All their fixtures will also be deleted.')) {
    // Reset immediately — confirm() dialog eats the mouseup, leaving the browser
    // stuck in a "button held" state. Must happen synchronously before any async work.
    window.dispatchEvent(new PointerEvent('pointercancel', { bubbles: true, pointerId: 1 }))
    window.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }))
    teamStore.deleteTeam(id)
  }
}
</script>

<template>
  <div class="fixed inset-0 bg-black/70 flex items-center justify-center z-50 px-4">
    <div
      class="bg-slate-800 rounded-lg shadow-2xl w-full max-w-lg border border-slate-600 overflow-hidden"
    >
      <div class="p-4 border-b border-slate-700 flex justify-between items-center bg-slate-900">
        <h3 class="text-lg font-bold text-white">Manage Teams</h3>
        <button @click="emit('close')" class="text-slate-400 hover:text-white text-xl leading-none">
          &times;
        </button>
      </div>
      <div class="p-6 space-y-6">
        <form @submit.prevent="handleAddTeam" class="flex gap-3">
          <input
            v-model="newTeamName"
            type="text"
            placeholder="Team name..."
            class="flex-1 bg-slate-900 border border-slate-600 rounded px-4 py-2 text-white focus:outline-none focus:border-emerald-500"
          />
          <button
            type="submit"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-500 text-white rounded font-bold"
          >
            Add
          </button>
        </form>
        <div class="max-h-60 overflow-y-auto space-y-2 pr-2">
          <div
            v-for="team in teamStore.teams"
            :key="team.id"
            class="flex justify-between items-center bg-slate-700 p-3 rounded border border-slate-600"
          >
            <span class="text-white font-medium">{{ team.name }}</span>
            <button
              @click="handleDeleteTeam(team.id)"
              class="text-red-400 hover:text-red-300 font-semibold text-sm"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
