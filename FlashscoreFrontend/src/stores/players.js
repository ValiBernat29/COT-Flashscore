import { defineStore } from 'pinia'
import { ref, nextTick, onMounted } from 'vue'

const API_URL = 'http://localhost:5198/api/players'


export const usePlayerStore = defineStore('players', () => {
  const players = ref([])

  const fetchPlayersByTeam = async (teamId) => {
    try {
      const res = await fetch(`${API_URL}/team/${teamId}`)
      players.value = await res.json()
    } catch (err) {
      console.error('Failed to fetch players:', err)
    }
  }

  const addPlayer = async (teamId, name, number, position) => {
    try {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          teamId: Number(teamId),
          name,
          number: Number(number),
          position,
        }),
      })
      const newPlayer = await res.json()
      players.value.push(newPlayer)
    } catch (err) {
      console.error('Failed to add player:', err)
    }
  }

  const deletePlayer = async (id) => {
    if (document.activeElement) document.activeElement.blur()

    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE' })
      if (!res.ok) {
        alert('Failed to delete player.')
        return
      }

      players.value = players.value.filter((p) => p.id !== id)

      await nextTick()
      window.dispatchEvent(new MouseEvent('mousemove', { bubbles: true }))
    } catch (err) {
      console.error('Failed to delete player:', err)
    }
  }

  return { players, fetchPlayersByTeam, addPlayer, deletePlayer }
})
