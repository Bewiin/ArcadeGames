# Neon Dodge — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter le jeu arcade Neon Dodge — route top-down, voiture du joueur, 3 types d'obstacles, 3 vies, musique chiptune Web Audio API — dans l'Arcade Hub.

**Architecture:** 1 Area `NeonDodge`, controller trivial `GET Index()` avec `[Authorize]`. Toute la logique de jeu est en JavaScript/Canvas dans la vue (IIFE, pas de dépendance externe). Pas de session, pas de service serveur.

**Tech Stack:** .NET 10, ASP.NET Core MVC, JavaScript ES6, Canvas 2D API, Web Audio API, localStorage (meilleur score).

---

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Projet-Groupe/Areas/NeonDodge/Controllers/NeonDodgeController.cs` | Controller trivial — `GET Index()` uniquement |
| `Projet-Groupe/Areas/NeonDodge/Views/_ViewImports.cshtml` | TagHelpers + using pour l'Area |
| `Projet-Groupe/Areas/NeonDodge/Views/_ViewStart.cshtml` | Layout partagé |
| `Projet-Groupe/Areas/NeonDodge/Views/NeonDodge/Index.cshtml` | Canvas 400×600 + tout le JS du jeu |
| `Projet-Groupe/Games/GamesCatalog.cs` | +1 ligne (NeonDodge) |

---

### Task 1 : Scaffolding Area + catalogue

**Files:**
- Create: `Projet-Groupe/Areas/NeonDodge/Controllers/NeonDodgeController.cs`
- Create: `Projet-Groupe/Areas/NeonDodge/Views/_ViewImports.cshtml`
- Create: `Projet-Groupe/Areas/NeonDodge/Views/_ViewStart.cshtml`
- Modify: `Projet-Groupe/Games/GamesCatalog.cs`

- [ ] **Step 1 : Créer le controller**

Create `Projet-Groupe/Areas/NeonDodge/Controllers/NeonDodgeController.cs` :
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.NeonDodge.Controllers;

[Area("NeonDodge")]
[Authorize]
public class NeonDodgeController : Controller
{
    public IActionResult Index() => View();
}
```

- [ ] **Step 2 : Créer _ViewImports.cshtml**

Create `Projet-Groupe/Areas/NeonDodge/Views/_ViewImports.cshtml` :
```cshtml
@using Projet_Groupe
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 3 : Créer _ViewStart.cshtml**

Create `Projet-Groupe/Areas/NeonDodge/Views/_ViewStart.cshtml` :
```cshtml
@{
    Layout = "_Layout";
}
```

- [ ] **Step 4 : Ajouter au catalogue**

Lire `Projet-Groupe/Games/GamesCatalog.cs`, ajouter après la dernière entrée :
```csharp
        new GameDescriptor(
            DisplayName: "Neon Dodge",
            Slug:        "neondodge",
            Description: "Évite les voitures, camions et barils sur la route néon.",
            Icon:        "🚗",
            Area:        "NeonDodge"),
```

- [ ] **Step 5 : Compiler**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj`
Expected: Build succeeded, 0 erreur.

- [ ] **Step 6 : Lancer les tests**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS — tous les tests existants + le slug "neondodge" est unique.

- [ ] **Step 7 : Commit**

```bash
git add Projet-Groupe/Areas/NeonDodge/Controllers Projet-Groupe/Areas/NeonDodge/Views/_ViewImports.cshtml Projet-Groupe/Areas/NeonDodge/Views/_ViewStart.cshtml Projet-Groupe/Games/GamesCatalog.cs
git commit -m "feat: scaffolding Area NeonDodge et entrée catalogue"
```

---

### Task 2 : Vue complète (Canvas + JS)

**Files:**
- Create: `Projet-Groupe/Areas/NeonDodge/Views/NeonDodge/Index.cshtml`

- [ ] **Step 1 : Créer la vue**

Create `Projet-Groupe/Areas/NeonDodge/Views/NeonDodge/Index.cshtml` :

```cshtml
@{
    ViewData["Title"] = "Neon Dodge";
}

<style>
    .nd-wrapper { display: flex; flex-direction: column; align-items: center; padding: 1rem 0; }
    #ndCanvas { border: 1px solid rgba(0,212,255,0.3); box-shadow: 0 0 30px rgba(0,212,255,0.15); display: block; }
    .nd-hint { color: var(--text-muted); font-family: 'Exo 2', sans-serif; font-size: .85rem; margin-top: .8rem; text-align: center; }
</style>

<h1 class="neon-pink text-center mb-3">🚗 NEON DODGE</h1>

<div class="nd-wrapper">
    <canvas id="ndCanvas" width="400" height="600" tabindex="0"></canvas>
    <p class="nd-hint">⬅️ ➡️ déplacer &nbsp;|&nbsp; Espace démarrer &nbsp;|&nbsp; M mute</p>
</div>

<script>
(function () {
    'use strict';

    // ── Constants ─────────────────────────────────────────────────────────────
    const CW = 400, CH = 600;
    const ROAD_L = 30, ROAD_R = CW - 30, ROAD_W = CW - 60;
    const LANE_COUNT = 3;
    const LANE_W = ROAD_W / LANE_COUNT;
    const PW = 40, PH = 60;
    const PLAYER_SPEED = 5;
    const INVINCIBLE_MS = 2000;
    const FLASH_MS = 500;

    const OBSTACLE_DEFS = {
        CAR:    { w: 40, h: 60, color: '#ff2d78', baseSpeed: 3.0, zigzag: false },
        TRUCK:  { w: 76, h: 90, color: '#b44fff', baseSpeed: 2.0, zigzag: false },
        BARREL: { w: 30, h: 30, color: '#ff8c00', baseSpeed: 4.0, zigzag: true  },
    };
    const SPAWN_POOL = ['CAR','CAR','CAR','TRUCK','BARREL'];

    const MELODY = [
        523.25, 659.25, 783.99, 659.25, 698.46, 587.33, 523.25, 523.25,
        493.88, 523.25, 587.33, 659.25, 523.25, 440.00, 0, 0
    ];

    // ── Canvas ────────────────────────────────────────────────────────────────
    const canvas = document.getElementById('ndCanvas');
    const ctx    = canvas.getContext('2d');
    canvas.focus();

    // ── State ─────────────────────────────────────────────────────────────────
    let player = { x: CW/2 - PW/2, y: CH - PH - 20 };
    let keys   = {};
    let dashOffset = 0;
    let muted      = false;
    let audioCtx   = null;
    let musicTimeout = null;

    let status, lives, score, bestScore, obstacles, speedMult,
        spawnInterval, lastSpawn, invincibleUntil, flashUntil,
        startTime, lastScoreUpdate, diffLevel;

    bestScore = parseInt(localStorage.getItem('neonDodgeBestScore') || '0');
    status = 'START';

    // ── Input ─────────────────────────────────────────────────────────────────
    window.addEventListener('keydown', e => {
        keys[e.code] = true;
        if (e.code === 'Space')   { e.preventDefault(); handleSpace(); }
        if (e.code === 'KeyM')    toggleMute();
        if (['ArrowLeft','ArrowRight'].includes(e.code)) e.preventDefault();
    });
    window.addEventListener('keyup', e => { keys[e.code] = false; });

    // ── Game control ──────────────────────────────────────────────────────────
    function handleSpace() {
        if (status === 'START' || status === 'GAMEOVER') startGame();
    }

    function startGame() {
        player.x        = CW/2 - PW/2;
        status          = 'PLAYING';
        lives           = 3;
        score           = 0;
        obstacles       = [];
        speedMult       = 1.0;
        spawnInterval   = 1500;
        diffLevel       = 0;
        const now       = performance.now();
        lastSpawn       = now;
        invincibleUntil = 0;
        flashUntil      = 0;
        startTime       = now;
        lastScoreUpdate = now;
        startMusic();
    }

    // ── Update ────────────────────────────────────────────────────────────────
    function update(ts) {
        if (keys['ArrowLeft'])  player.x = Math.max(ROAD_L,        player.x - PLAYER_SPEED);
        if (keys['ArrowRight']) player.x = Math.min(ROAD_R - PW,   player.x + PLAYER_SPEED);

        dashOffset = (dashOffset + 4 * speedMult) % 40;

        const elapsedSec = (ts - startTime) / 1000;
        const newLevel   = Math.floor(elapsedSec / 10);
        if (newLevel > diffLevel) {
            diffLevel      = newLevel;
            speedMult      *= 1.1;
            spawnInterval  = Math.max(400, spawnInterval * 0.9);
        }

        if (ts - lastScoreUpdate >= 1000) {
            score           += 10;
            lastScoreUpdate += 1000;
        }

        if (ts - lastSpawn >= spawnInterval) {
            spawnObstacle();
            lastSpawn = ts;
        }

        for (const obs of obstacles) {
            obs.y += obs.speed * speedMult;
            if (obs.zigzag) {
                obs.x += obs.vx;
                if (obs.x <= ROAD_L || obs.x + obs.w >= ROAD_R) obs.vx *= -1;
            }
        }

        obstacles = obstacles.filter(o => o.y < CH + 120);

        if (ts > invincibleUntil) {
            for (const obs of obstacles) {
                if (collides(player.x, player.y, PW, PH, obs.x, obs.y, obs.w, obs.h)) {
                    onCollision(ts);
                    break;
                }
            }
        }
    }

    function spawnObstacle() {
        const type = SPAWN_POOL[Math.floor(Math.random() * SPAWN_POOL.length)];
        const def  = OBSTACLE_DEFS[type];
        const x    = ROAD_L + Math.random() * (ROAD_W - def.w);
        obstacles.push({
            type, x, y: -def.h - 10,
            w: def.w, h: def.h,
            speed: def.baseSpeed,
            color: def.color,
            zigzag: def.zigzag,
            vx: def.zigzag ? (Math.random() > 0.5 ? 1.5 : -1.5) : 0,
        });
    }

    function collides(ax, ay, aw, ah, bx, by, bw, bh) {
        return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
    }

    function onCollision(ts) {
        lives--;
        flashUntil      = ts + FLASH_MS;
        invincibleUntil = ts + INVINCIBLE_MS;
        if (lives <= 0) {
            status = 'GAMEOVER';
            if (score > bestScore) {
                bestScore = score;
                localStorage.setItem('neonDodgeBestScore', String(score));
            }
            stopMusic();
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────
    function render(ts) {
        ctx.fillStyle = '#07070f';
        ctx.fillRect(0, 0, CW, CH);

        drawRoad();

        if (status !== 'START') {
            obstacles.forEach(drawObstacle);
            drawPlayer(ts);
            drawHUD();
        }

        if (status === 'START')    drawOverlay('START');
        if (status === 'GAMEOVER') drawOverlay('GAMEOVER');
    }

    function drawRoad() {
        ctx.fillStyle = '#0b0b1a';
        ctx.fillRect(ROAD_L, 0, ROAD_W, CH);

        ctx.save();
        ctx.shadowColor = '#ff2d78';
        ctx.shadowBlur  = 10;
        ctx.fillStyle   = '#ff2d78';
        ctx.fillRect(ROAD_L - 5, 0, 5, CH);
        ctx.fillRect(ROAD_R,     0, 5, CH);
        ctx.restore();

        ctx.save();
        ctx.strokeStyle = 'rgba(255,255,255,0.22)';
        ctx.lineWidth   = 2;
        for (let lane = 1; lane < LANE_COUNT; lane++) {
            const lx = ROAD_L + lane * LANE_W;
            ctx.beginPath();
            for (let y = dashOffset - 40; y < CH + 40; y += 40) {
                ctx.moveTo(lx, y);
                ctx.lineTo(lx, y + 20);
            }
            ctx.stroke();
        }
        ctx.restore();
    }

    function drawPlayer(ts) {
        let color = '#00d4ff';
        if (ts < flashUntil) {
            color = '#ff2d78';
        } else if (ts < invincibleUntil && Math.floor(ts / 150) % 2 === 0) {
            return;
        }
        ctx.save();
        ctx.shadowColor = color;
        ctx.shadowBlur  = 18;
        ctx.fillStyle   = color;
        ctx.fillRect(player.x + 6,  player.y + 5, PW - 12, PH - 5);
        ctx.fillRect(player.x + 10, player.y,     PW - 20, 10);
        ctx.fillStyle = 'rgba(7,7,15,0.55)';
        ctx.fillRect(player.x + 9,  player.y + 12, PW - 18, PH * 0.3);
        ctx.restore();
    }

    function drawObstacle(obs) {
        ctx.save();
        ctx.shadowColor = obs.color;
        ctx.shadowBlur  = 12;
        ctx.fillStyle   = obs.color;
        if (obs.type === 'BARREL') {
            ctx.beginPath();
            ctx.ellipse(obs.x + obs.w/2, obs.y + obs.h/2, obs.w/2, obs.h/2, 0, 0, Math.PI*2);
            ctx.fill();
            ctx.strokeStyle = 'rgba(0,0,0,0.4)';
            ctx.lineWidth   = 4;
            ctx.beginPath();
            ctx.moveTo(obs.x + 5,         obs.y + obs.h/2);
            ctx.lineTo(obs.x + obs.w - 5, obs.y + obs.h/2);
            ctx.stroke();
        } else {
            ctx.fillRect(obs.x, obs.y, obs.w, obs.h);
            ctx.fillStyle = 'rgba(0,0,0,0.4)';
            const wh = obs.type === 'TRUCK' ? obs.h * 0.25 : obs.h * 0.3;
            ctx.fillRect(obs.x + 6, obs.y + 8, obs.w - 12, wh);
        }
        ctx.restore();
    }

    function drawHUD() {
        ctx.save();
        ctx.font = '18px Arial';
        ctx.fillText('❤️'.repeat(lives) + '🖤'.repeat(3 - lives), 8, 28);

        ctx.fillStyle   = '#00d4ff';
        ctx.shadowColor = '#00d4ff';
        ctx.shadowBlur  = 8;
        ctx.font        = 'bold 14px Orbitron, monospace';
        ctx.textAlign   = 'center';
        ctx.fillText(`SCORE : ${score}`, CW/2, 28);

        ctx.shadowBlur  = 0;
        ctx.font        = '18px Arial';
        ctx.textAlign   = 'right';
        ctx.fillText(muted ? '🔇' : '🔊', CW - 8, 28);
        ctx.restore();
    }

    function drawOverlay(type) {
        ctx.save();
        ctx.fillStyle = 'rgba(7,7,15,0.88)';
        ctx.fillRect(0, 0, CW, CH);
        ctx.textAlign = 'center';

        if (type === 'START') {
            ctx.shadowColor = '#ff2d78';
            ctx.shadowBlur  = 24;
            ctx.fillStyle   = '#ff2d78';
            ctx.font        = 'bold 38px Orbitron, monospace';
            ctx.fillText('NEON DODGE', CW/2, CH/2 - 90);

            ctx.shadowColor = '#00d4ff';
            ctx.shadowBlur  = 8;
            ctx.fillStyle   = '#00d4ff';
            ctx.font        = '12px Orbitron, monospace';
            ctx.fillText('ÉVITE VOITURES 🚗  CAMIONS 🚛  BARILS 🛢️', CW/2, CH/2 - 40);
            ctx.fillText('3 VIES  —  SCORE : TEMPS × 10', CW/2, CH/2 - 15);

            ctx.shadowColor = '#ff2d78';
            ctx.shadowBlur  = 16;
            ctx.fillStyle   = '#ff2d78';
            ctx.font        = 'bold 16px Orbitron, monospace';
            ctx.fillText('— ESPACE POUR DÉMARRER —', CW/2, CH/2 + 45);

            if (bestScore > 0) {
                ctx.fillStyle   = '#b44fff';
                ctx.shadowColor = '#b44fff';
                ctx.shadowBlur  = 8;
                ctx.font        = '12px Orbitron, monospace';
                ctx.fillText(`MEILLEUR : ${bestScore}`, CW/2, CH/2 + 80);
            }
        }

        if (type === 'GAMEOVER') {
            ctx.shadowColor = '#ff2d78';
            ctx.shadowBlur  = 24;
            ctx.fillStyle   = '#ff2d78';
            ctx.font        = 'bold 38px Orbitron, monospace';
            ctx.fillText('GAME OVER', CW/2, CH/2 - 80);

            ctx.shadowColor = '#00d4ff';
            ctx.shadowBlur  = 10;
            ctx.fillStyle   = '#00d4ff';
            ctx.font        = 'bold 22px Orbitron, monospace';
            ctx.fillText(`SCORE : ${score}`, CW/2, CH/2 - 20);

            ctx.fillStyle   = '#b44fff';
            ctx.shadowColor = '#b44fff';
            ctx.shadowBlur  = 8;
            ctx.font        = '14px Orbitron, monospace';
            ctx.fillText(`MEILLEUR : ${bestScore}`, CW/2, CH/2 + 20);

            ctx.shadowColor = '#ff2d78';
            ctx.shadowBlur  = 16;
            ctx.fillStyle   = '#ff2d78';
            ctx.font        = 'bold 16px Orbitron, monospace';
            ctx.fillText('— ESPACE POUR REJOUER —', CW/2, CH/2 + 70);
        }

        ctx.restore();
    }

    // ── Music (Web Audio API) ─────────────────────────────────────────────────
    function startMusic() {
        if (muted) return;
        stopMusic();
        try {
            audioCtx = new (window.AudioContext || window.webkitAudioContext)();
            scheduleMelody();
        } catch (e) { /* audio non supporté */ }
    }

    function scheduleMelody() {
        if (!audioCtx || muted) return;
        const bpm     = 170;
        const beatLen = 60 / bpm;
        let   time    = audioCtx.currentTime + 0.05;

        MELODY.forEach((freq, i) => {
            if (freq === 0) return;
            const osc  = audioCtx.createOscillator();
            const gain = audioCtx.createGain();
            osc.connect(gain);
            gain.connect(audioCtx.destination);
            osc.type          = 'square';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0.07, time + i * beatLen);
            gain.gain.exponentialRampToValueAtTime(0.001, time + i * beatLen + beatLen * 0.75);
            osc.start(time + i * beatLen);
            osc.stop( time + i * beatLen + beatLen);
        });

        const loopMs = MELODY.length * beatLen * 1000;
        musicTimeout = setTimeout(() => {
            if (audioCtx && !muted && status === 'PLAYING') scheduleMelody();
        }, loopMs - 100);
    }

    function stopMusic() {
        clearTimeout(musicTimeout);
        if (audioCtx) { audioCtx.close().catch(() => {}); audioCtx = null; }
    }

    function toggleMute() {
        muted = !muted;
        if (muted) stopMusic();
        else if (status === 'PLAYING') startMusic();
    }

    // ── Game loop ─────────────────────────────────────────────────────────────
    function loop(ts) {
        if (status === 'PLAYING') update(ts);
        render(ts);
        requestAnimationFrame(loop);
    }
    requestAnimationFrame(loop);

})();
</script>
```

- [ ] **Step 2 : Compiler**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj`
Expected: Build succeeded, 0 erreur.

- [ ] **Step 3 : Commit**

```bash
git add Projet-Groupe/Areas/NeonDodge/Views/NeonDodge
git commit -m "feat: ajoute la vue et le jeu Neon Dodge (Canvas + Web Audio)"
```

---

### Task 3 : Vérification manuelle

- [ ] **Step 1 : Lancer tous les tests**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS — tous les tests existants (slug "neondodge" unique confirmé).

- [ ] **Step 2 : Vérifier dans le navigateur**

Run: `dotnet run --project Projet-Groupe/Projet-Groupe.csproj`

Checklist :
- `/Games` → carte "🚗 Neon Dodge" visible
- Clic sans login → redirect login
- Après login → `/NeonDodge/NeonDodge` → écran START avec le titre
- ESPACE → jeu démarre, voiture visible, obstacles arrivent
- Flèches ⬅️ ➡️ → voiture se déplace
- Collision → flash rouge, vie perdue (❤️ → 🖤)
- 3 collisions → GAME OVER avec score
- ESPACE → restart
- M → 🔊 / 🔇 toggle
- Score augmente toutes les secondes
- Vitesse augmente après 10s

- [ ] **Step 3 : Commit doc**

```bash
git add docs/superpowers/plans/2026-06-16-neon-dodge.md
git commit -m "docs: plan implémentation Neon Dodge"
```
