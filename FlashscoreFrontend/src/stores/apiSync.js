import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_URL = 'http://localhost:5198/api/sync'

export const useApiSyncStore = defineStore('apiSync', () => {
  const isSyncing = ref(false)
  const lastSyncTime = ref(null)
  const syncError = ref(null)
  const syncMessage = ref(null)

  const syncTeams = async () => {
    isSyncing.value = true
    syncError.value = null
    syncMessage.value = null

    try {
      const res = await fetch(API_URL + '/teams', { method: 'POST' })
      const data = await res.json()

      if (!res.ok) {
        syncError.value = data.error || 'Sync failed. Check the backend logs.'
      } else {
        syncMessage.value = data.message || 'Sync complete!'
        lastSyncTime.value = new Date().toLocaleTimeString()
      }
    } catch (err) {
      syncError.value = 'Could not reach the backend. Is it running?'
      console.error('Sync error:', err)
    } finally {
      isSyncing.value = false
    }
  }

  return { isSyncing, lastSyncTime, syncError, syncMessage, syncTeams }
})
