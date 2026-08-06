import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_URL = 'http://localhost:5198/api/teams'

export const useTeamStore = defineStore('teams', () => {
  const teams = ref([])

  const fetchTeams = async () => {
    try {
      const res = await fetch(API_URL)
      teams.value = await res.json()
    } catch (err) {
      console.error('Failed to fetch teams:', err)
    }
  }

  const addTeam = async (name) => {
    try {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      const newTeam = await res.json()
      teams.value.push(newTeam)
    } catch (err) {
      console.error('Failed to add team:', err)
    }
  }

  const deleteTeam = async (id) => {
    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE' })

      if (!res.ok) {
        alert('Cannot delete this team! Make sure it is not part of any existing fixtures.')
        window.dispatchEvent(new PointerEvent('pointercancel', { bubbles: true, pointerId: 1 }))
        window.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }))
        return
      }

      teams.value = teams.value.filter((t) => t.id !== id)
    } catch (err) {
      console.error('Failed to delete team:', err)
    }
  }

  return { teams, fetchTeams, addTeam, deleteTeam }
})
