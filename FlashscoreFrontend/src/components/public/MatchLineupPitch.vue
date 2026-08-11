<script setup>
import { computed } from 'vue'

const props = defineProps({
  match: { type: Object, required: true },
  homeRoster: { type: Array, default: () => [] },
  awayRoster: { type: Array, default: () => [] },
  homeTeamName: { type: String, default: 'Home' },
  awayTeamName: { type: String, default: 'Away' },
})

// Group players into [GK, DEF, MID, FWD] — any unrecognised position falls into FWD (catch-all)
const getFormationGroups = (lineupIds, roster) => {
  if (!lineupIds || lineupIds.length === 0) return [[], [], [], []]

  const startingXI = roster.filter(p => lineupIds.includes(p.id))

  const gk  = startingXI.filter(p => /goal|\bgk\b/i.test(p.position ?? ''))
  const def = startingXI.filter(p => /def|\bcb\b|\blb\b|\brb\b|\bwb\b/i.test(p.position ?? ''))
  const mid = startingXI.filter(p => /mid|\bcm\b|\bdm\b|\bam\b|\bcam\b|\bcdm\b/i.test(p.position ?? ''))
  // Catch-all: anyone not already bucketed goes into forwards
  const bucketed = new Set([...gk, ...def, ...mid].map(p => p.id))
  const fwd = startingXI.filter(p => !bucketed.has(p.id))

  return [gk, def, mid, fwd]
}

const homeFormation = computed(() => getFormationGroups(props.match.homeLineup, props.homeRoster))
const awayFormation = computed(() => getFormationGroups(props.match.awayLineup, props.awayRoster))

// Build "4-3-3" style string from the non-GK rows
const buildFormationStr = (groups, reverse = false) => {
  const parts = groups
    .slice(1)                   // skip GK row
    .filter(g => g.length > 0)
    .map(g => g.length)
  if (reverse) parts.reverse() // away side: show FWD-MID-DEF to mirror the pitch
  return parts.join('-')
}

const homeFormationStr = computed(() => buildFormationStr(homeFormation.value))
const awayFormationStr = computed(() => buildFormationStr(awayFormation.value, true))

const hasLineup = computed(
  () =>
    (props.match.homeLineup?.length > 0 && props.homeRoster.length > 0) ||
    (props.match.awayLineup?.length > 0 && props.awayRoster.length > 0)
)

const getLastName = (name) => name?.split(' ').pop() ?? name
</script>

<template>
  <div class="lineup-pitch-wrap">

    <div class="formation-header">
      <div class="formation-team home-side">
        <span class="formation-label">{{ homeFormationStr || '—' }}</span>
        <span class="formation-name">{{ homeTeamName }}</span>
      </div>
      <div class="formation-center-label">FORMATION</div>
      <div class="formation-team away-side">
        <span class="formation-name">{{ awayTeamName }}</span>
        <span class="formation-label">{{ awayFormationStr || '—' }}</span>
      </div>
    </div>

    <div class="pitch-outer">
      <div class="pitch">

        <!-- Pitch Markings -->
        <div class="pitch-markings">
          <div class="grass-stripe" v-for="i in 7" :key="i" :style="{ left: ((i-1)/7 * 100) + '%', width: (1/7 * 100) + '%', opacity: i % 2 === 0 ? 0.06 : 0 }"></div>

          <div class="center-line"></div>
          <div class="center-circle"></div>
          <div class="center-dot"></div>

          <div class="penalty-box left-box"></div>
          <div class="six-yard left-six"></div>
          <div class="penalty-spot left-spot"></div>
          <div class="penalty-arc left-arc"></div>

          <div class="penalty-box right-box"></div>
          <div class="six-yard right-six"></div>
          <div class="penalty-spot right-spot"></div>
          <div class="penalty-arc right-arc"></div>

          <div class="corner-arc top-left-arc"></div>
          <div class="corner-arc top-right-arc"></div>
          <div class="corner-arc bottom-left-arc"></div>
          <div class="corner-arc bottom-right-arc"></div>
        </div>

        <div class="team-half home-half">
          <div
            v-for="(group, gi) in homeFormation"
            :key="'hg-' + gi"
            class="formation-row"
          >
            <div
              v-for="player in group"
              :key="player.id"
              class="player-token-wrap"
            >
              <div class="player-token home-token">
                <span class="jersey-number">{{ player.number }}</span>
                <div class="player-tooltip">
                  <span class="tooltip-number">#{{ player.number }}</span>
                  <span class="tooltip-name">{{ player.name }}</span>
                  <span class="tooltip-pos">{{ player.position }}</span>
                </div>
              </div>
              <span class="player-last-name">{{ getLastName(player.name) }}</span>
            </div>
          </div>
        </div>

        <div class="team-half away-half">
          <div
            v-for="(group, gi) in awayFormation"
            :key="'ag-' + gi"
            class="formation-row"
          >
            <div
              v-for="player in group"
              :key="player.id"
              class="player-token-wrap"
            >
              <div class="player-token away-token">
                <span class="jersey-number">{{ player.number }}</span>
                <div class="player-tooltip tooltip-left">
                  <span class="tooltip-number">#{{ player.number }}</span>
                  <span class="tooltip-name">{{ player.name }}</span>
                  <span class="tooltip-pos">{{ player.position }}</span>
                </div>
              </div>
              <span class="player-last-name">{{ getLastName(player.name) }}</span>
            </div>
          </div>
        </div>

      </div>
    </div>

    <div v-if="!hasLineup" class="no-lineup-msg">
      <span>Lineup not yet announced</span>
    </div>

  </div>
</template>

<style scoped>
/*  Wrapper  */
.lineup-pitch-wrap {
  background: #0f1923;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(255,255,255,0.08);
  user-select: none;
}

/*  Formation header  */
.formation-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 20px;
  background: #141e2b;
  border-bottom: 1px solid rgba(255,255,255,0.07);
  font-family: 'Inter', system-ui, sans-serif;
}
.formation-team {
  display: flex;
  align-items: center;
  gap: 10px;
}
.home-side { flex-direction: row; }
.away-side { flex-direction: row-reverse; }

.formation-label {
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 1px;
  color: #e2e8f0;
  background: rgba(255,255,255,0.07);
  padding: 3px 10px;
  border-radius: 6px;
}
.formation-name {
  font-size: 12px;
  font-weight: 600;
  color: #94a3b8;
  max-width: 120px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.formation-center-label {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 2px;
  color: #475569;
  text-transform: uppercase;
}

/*  Pitch outer shell  */
.pitch-outer {
  padding: 12px;
  overflow-x: auto;
}

/*  Pitch  */
.pitch {
  position: relative;
  min-width: 640px;
  aspect-ratio: 16 / 9;
  background: #2d7a3a;
  border: 2px solid rgba(255,255,255,0.25);
  border-radius: 4px;
  display: flex;
  overflow: hidden;
  box-shadow: inset 0 0 60px rgba(0,0,0,0.35);
}

/*  Pitch Markings  */
.pitch-markings {
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 1;
}

.grass-stripe {
  position: absolute;
  top: 0; bottom: 0;
  background: rgba(0,0,0,1);
}

.center-line {
  position: absolute;
  top: 0; bottom: 0;
  left: 50%;
  width: 1.5px;
  background: rgba(255,255,255,0.5);
  transform: translateX(-50%);
}
.center-circle {
  position: absolute;
  top: 50%; left: 50%;
  width: 17%;
  aspect-ratio: 1;
  border: 1.5px solid rgba(255,255,255,0.5);
  border-radius: 50%;
  transform: translate(-50%, -50%);
}
.center-dot {
  position: absolute;
  top: 50%; left: 50%;
  width: 6px; height: 6px;
  background: rgba(255,255,255,0.6);
  border-radius: 50%;
  transform: translate(-50%, -50%);
}

/* Penalty boxes */
.penalty-box {
  position: absolute;
  top: 20%; bottom: 20%;
  width: 14%;
  border: 1.5px solid rgba(255,255,255,0.5);
}
.left-box  { left: 0; border-left: none; }
.right-box { right: 0; border-right: none; }

.six-yard {
  position: absolute;
  top: 35%; bottom: 35%;
  width: 5%;
  border: 1.5px solid rgba(255,255,255,0.5);
}
.left-six  { left: 0; border-left: none; }
.right-six { right: 0; border-right: none; }

.penalty-spot {
  position: absolute;
  top: 50%;
  width: 5px; height: 5px;
  background: rgba(255,255,255,0.55);
  border-radius: 50%;
  transform: translateY(-50%);
}
.left-spot  { left: 10%; }
.right-spot { right: 10%; }

.penalty-arc {
  position: absolute;
  top: 50%;
  width: 80px; height: 80px;
  border: 1.5px solid rgba(255,255,255,0.5);
  border-radius: 50%;
  transform: translateY(-50%);
  clip-path: none;
}
.left-arc {
  left: calc(14% - 40px);
  clip-path: inset(0 0 0 50%);
}
.right-arc {
  right: calc(14% - 40px);
  clip-path: inset(0 50% 0 0);
}

.corner-arc {
  position: absolute;
  width: 18px; height: 18px;
  border: 1.5px solid rgba(255,255,255,0.5);
  border-radius: 50%;
}
.top-left-arc     { top: -9px; left: -9px; }
.top-right-arc    { top: -9px; right: -9px; }
.bottom-left-arc  { bottom: -9px; left: -9px; }
.bottom-right-arc { bottom: -9px; right: -9px; }

/*  Team halves  */
.team-half {
  position: relative;
  width: 50%;
  display: flex;
  flex-direction: row;
  align-items: stretch;
  padding: 12px 6px;
  z-index: 2;
}

/* Home: GK at left edge, FWD near center */
.home-half {
  flex-direction: row;
}
/* Away: GK at right edge, FWD near center (reversed) */
.away-half {
  flex-direction: row-reverse;
}

/* Each column = one position group */
.formation-row {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-evenly;
  gap: 4px;
}

/*  Player token  */
.player-token-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  cursor: default;
  position: relative;
}

.player-token {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.85);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  box-shadow: 0 2px 8px rgba(0,0,0,0.5);
}
.player-token-wrap:hover .player-token {
  transform: scale(1.18);
  box-shadow: 0 4px 16px rgba(0,0,0,0.7);
  z-index: 10;
}

.home-token {
  background: radial-gradient(circle at 40% 35%, #5b9dff, #2563eb);
}
.away-token {
  background: radial-gradient(circle at 40% 35%, #ff7575, #dc2626);
}

.jersey-number {
  font-size: 10px;
  font-weight: 900;
  color: #fff;
  line-height: 1;
  text-shadow: 0 1px 3px rgba(0,0,0,0.6);
  font-family: 'Inter', system-ui, sans-serif;
}

.player-last-name {
  font-size: 9px;
  font-weight: 700;
  color: #fff;
  background: rgba(10, 20, 35, 0.82);
  padding: 1px 6px;
  border-radius: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 72px;
  text-align: center;
  font-family: 'Inter', system-ui, sans-serif;
  letter-spacing: 0.3px;
}

/*  Tooltip  */
.player-tooltip {
  display: none;
  position: absolute;
  bottom: calc(100% + 8px);
  left: 50%;
  transform: translateX(-50%);
  background: #0f1923;
  border: 1px solid rgba(255,255,255,0.15);
  border-radius: 8px;
  padding: 6px 10px;
  flex-direction: column;
  gap: 2px;
  white-space: nowrap;
  z-index: 50;
  box-shadow: 0 4px 20px rgba(0,0,0,0.7);
  pointer-events: none;
}
.tooltip-left {
  left: auto;
  right: 50%;
  transform: translateX(50%);
}
.player-token-wrap:hover .player-tooltip {
  display: flex;
}

.tooltip-number {
  font-size: 10px;
  font-weight: 700;
  color: #64748b;
  font-family: 'Inter', system-ui, sans-serif;
}
.tooltip-name {
  font-size: 12px;
  font-weight: 800;
  color: #f1f5f9;
  font-family: 'Inter', system-ui, sans-serif;
}
.tooltip-pos {
  font-size: 10px;
  font-weight: 600;
  color: #475569;
  font-family: 'Inter', system-ui, sans-serif;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/*  No lineup fallback  */
.no-lineup-msg {
  text-align: center;
  padding: 16px;
  color: #475569;
  font-size: 13px;
  font-style: italic;
  font-family: 'Inter', system-ui, sans-serif;
}
</style>