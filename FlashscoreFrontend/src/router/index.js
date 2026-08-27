import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('../layouts/PublicLayout.vue'),
      children: [
        { path: '', name: 'standings', component: () => import('../views/StandingsView.vue') },
        { path: 'matches', name: 'matches', component: () => import('../views/MatchdayView.vue') },
        { path: 'matches/:id', name: 'match-detail', component: () => import('../views/MatchDetailView.vue') },
        { path: 'teams/:id', name: 'team-squad', component: () => import('../views/TeamSquadView.vue') },
      ],
    },
    {
      path: '/admin',
      component: () => import('../layouts/AdminLayout.vue'),
      children: [
        { path: '', name: 'dashboard', component: () => import('../views/LiveMatchDashboard.vue') },
        { path: 'rosters', name: 'rosters', component: () => import('../views/RosterView.vue') },
      ],
    },
  ],
})

export default router
