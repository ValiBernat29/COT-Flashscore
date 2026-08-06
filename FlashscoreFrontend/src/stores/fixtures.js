import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

const API_URL = 'http://localhost:5198/api/fixtures'

export const useFixtureStore = defineStore('fixtures', () => {
  const fixtures = ref([])

  const fetchFixtures = async () => {
    try {
      const res = await fetch(API_URL)
      fixtures.value = await res.json()
    } catch (err) {
      console.error('Failed to fetch fixtures:', err)
    }
  }

  // Grouping matches by matchday for the UI
  const matchesByMatchday = computed(() => {
    const groups = {}
    fixtures.value.forEach((match) => {
      if (!groups[match.matchday]) {
        groups[match.matchday] = []
      }
      groups[match.matchday].push(match)
    })
    return Object.keys(groups).map((day) => ({
      matchday: Number(day),
      matches: groups[day],
    }))
  })

  const getMatchById = (id) => fixtures.value.find((m) => m.id === id)

  const addFixture = async (matchday, homeTeamId, awayTeamId) => {
    try {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          matchday: Number(matchday),
          homeTeamId: Number(homeTeamId),
          awayTeamId: Number(awayTeamId),
          homeScore: 0,
          awayScore: 0,
          status: 'Scheduled',
        }),
      })
      await fetchFixtures()
    } catch (err) {
      console.error('Failed to add fixture:', err)
    }
  }

  const updateFixture = async (id, updatedData) => {
    try {
      const res = await fetch(`${API_URL}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedData),
      })

      if (!res.ok) {
        console.error('Failed to update fixture')
        return
      }

      await fetchFixtures()
    } catch (err) {
      console.error('Error updating fixture:', err)
    }
  }

  const deleteFixture = async (id) => {
    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE' })

      if (!res.ok) {
        alert('Failed to delete the fixture.')
        window.dispatchEvent(new PointerEvent('pointercancel', { bubbles: true, pointerId: 1 }))
        window.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }))
        return
      }

      fixtures.value = fixtures.value.filter((m) => m.id !== id)
    } catch (err) {
      console.error('Failed to delete fixture:', err)
    }
  }
  return {
    fixtures,
    matchesByMatchday,
    fetchFixtures,
    getMatchById,
    addFixture,
    updateFixture,
    deleteFixture,
  }
})
