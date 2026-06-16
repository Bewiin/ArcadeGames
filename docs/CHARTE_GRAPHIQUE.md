# Charte graphique — Arcade Hub

> Document de référence pour conserver la cohérence visuelle du projet.
> Toute nouvelle vue ou composant doit respecter ces règles.

---

## 1. Identité visuelle

**Nom affiché :** ARCADE HUB  
**Ambiance :** Salle d'arcade des années 80–90, esthétique néon sur fond sombre.  
**Mots clés :** flashy mais lisible, rétrofuturiste, énergique, immersif.

---

## 2. Palette de couleurs

| Rôle               | Variable CSS          | Valeur hex  |
|--------------------|-----------------------|-------------|
| Accent principal   | `--neon-pink`         | `#ff2d78`   |
| Accent secondaire  | `--neon-blue`         | `#00d4ff`   |
| Accent tertiaire   | `--neon-purple`       | `#b44fff`   |
| Fond global        | `--bg-dark`           | `#07070f`   |
| Fond des cartes    | `--bg-card`           | `#0f0f1a`   |
| Fond navbar/footer | `--bg-nav`            | `#08080f`   |
| Texte principal    | `--text-primary`      | `#e8e8f0`   |
| Texte secondaire   | `--text-muted`        | `#7777aa`   |

### Effets glow (box-shadow / text-shadow)

```css
--glow-pink: 0 0 8px #ff2d78, 0 0 24px rgba(255,45,120,0.45);
--glow-blue: 0 0 8px #00d4ff, 0 0 24px rgba(0,212,255,0.45);
```

**Règle :** ne jamais utiliser de fond blanc ou de couleur claire sur le corps de la page. Toujours rester dans les tons sombres (`#07070f` → `#1a1a2e` max).

---

## 3. Typographie

| Usage         | Police          | Fallback         | Poids utilisés    |
|---------------|-----------------|------------------|-------------------|
| Titres / marque / labels arcade | `Orbitron` (Google Fonts) | `monospace` | 400, 600, 700, 900 |
| Corps de texte / nav / paragraphes | `Exo 2` (Google Fonts) | `Segoe UI, sans-serif` | 300, 400, 600 |

**Import Google Fonts (déjà dans site.css) :**
```css
@import url('https://fonts.googleapis.com/css2?family=Orbitron:wght@400;600;700;900&family=Exo+2:wght@300;400;600&display=swap');
```

**Règles :**
- Tous les `h1`–`h6` utilisent **Orbitron**.
- Le corps de texte, les liens de nav et les paragraphes utilisent **Exo 2**.
- Les boutons utilisent **Orbitron** en taille réduite (`0.78rem`) avec `letter-spacing: 1.5px` et `text-transform: uppercase`.
- Ne jamais utiliser une police serif sur ce projet.

---

## 4. Mise en page

- **Fond de page :** grille subtile en `rgba(0,212,255,0.025)` sur fond `--bg-dark`, taille 44×44 px.
- **Sticky footer :** `body { display:flex; flex-direction:column; min-height:100vh }` + `body > .container { flex: 1 0 auto }`.
- **Conteneur principal :** classe Bootstrap `.container` standard.
- **Espacement des grilles de cartes :** `.row.g-4` (gap de 1.5rem).

---

## 5. Composants

### Navbar
- Fond : `--bg-nav` (`#08080f`)
- Bordure basse : `1px solid rgba(0,212,255,0.35)` + `box-shadow` bleu diffus
- **Brand :** Orbitron 900, couleur `--neon-pink`, `text-shadow: var(--glow-pink)`, animation `.neon-flicker`
- **Liens :** Exo 2 600, majuscules, `letter-spacing: 1.5px` — underline animée en bleu néon au hover

### Footer
- Même fond et bordure que la navbar (symétrie haut/bas)
- Texte en `--text-muted`, liens en `--neon-blue` → `--neon-pink` au hover
- Typographie réduite (`font-size: 0.8rem`)

### Cards (catalogues de jeux)
- Fond : `--bg-card`, bordure `rgba(0,212,255,0.2)`, `border-radius: 10px`
- Hover : `translateY(-5px)`, bordure `--neon-pink`, `box-shadow` rose diffus
- Titre (`.card-title`) : Orbitron, couleur `--neon-blue`, léger `text-shadow` bleu
- Description (`.card-text`) : couleur `--text-muted`
- L'icône du jeu est affichée au-dessus du titre, taille `2rem`

### Boutons primaires
- Dégradé : `linear-gradient(135deg, --neon-pink, --neon-purple)`
- Pas de border, Orbitron, majuscules, `letter-spacing: 1.5px`
- Hover : `box-shadow: var(--glow-pink)` + `translateY(-2px)`

### Formulaires
- Fond : `#0d0d1a`, bordure `rgba(0,212,255,0.3)`
- Focus : bordure `--neon-blue`, `box-shadow` bleu léger
- Labels en `--text-primary`
- Checkboxes cochées : fond `--neon-pink`

---

## 6. Classes utilitaires (site.css)

| Classe           | Effet                                                  |
|------------------|--------------------------------------------------------|
| `.neon-pink`     | Texte rose néon + `text-shadow` glow rose              |
| `.neon-blue`     | Texte bleu néon + `text-shadow` glow bleu              |
| `.neon-divider`  | `<hr>` dégradé transparent → bleu → rose → transparent |
| `.neon-pulse`    | Animation pulsation rose (2.5s, infinie)               |
| `.neon-flicker`  | Animation scintillement subtil (5s, infinie)           |

---

## 7. Icônes et illustrations

- Icônes : emojis Unicode uniquement (pas de librairie externe).
- Pas d'images décoratives ; l'ambiance est entièrement portée par CSS (glow, grille, dégradés).

---

## 8. Ton rédactionnel de l'interface

- Textes courts, impactants, en majuscules pour les labels importants.
- Références arcade bienvenues : "INSÉREZ UNE PIÈCE", "JOUER", "PLAYER 1".
- Langue : **français** par défaut pour les textes UI.

---

## 9. Ce qu'il ne faut PAS faire

- Ne pas utiliser de fond blanc ou clair (`#fff`, `bg-white`, `bg-light`).
- Ne pas utiliser la palette Bootstrap par défaut (primary bleu Bootstrap, etc.) sans la surcharger.
- Ne pas utiliser de shadows grises / neutres : préférer les shadows colorées (rose ou bleu).
- Ne pas supprimer les variables CSS `:root` — elles sont la source de vérité du thème.
- Ne pas ajouter de polices supplémentaires sans accord : deux polices suffisent.
