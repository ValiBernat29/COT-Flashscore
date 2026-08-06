<script setup>
import { onMounted } from 'vue'
import { RouterView } from 'vue-router'
import { useTeamStore } from './stores/teams'
import { useFixtureStore } from './stores/fixtures'

const teamStore = useTeamStore()
const fixtureStore = useFixtureStore()

onMounted(() => {
  teamStore.fetchTeams()
  fixtureStore.fetchFixtures()
})
</script>

<template>
  <RouterView />
</template>
