# 🏃 Unity 2D Endless Runner — Setup Guide

## Scripts Overview

| Script | Attach To | Purpose |
|---|---|---|
| `GameManager.cs` | Empty GameObject "GameManager" | Speed, score, game-over logic |
| `PlayerController.cs` | Player sprite | Jump, death, collision |
| `GroundSpawner.cs` | Empty GameObject "Spawner" | Spawns ground tiles & obstacles |
| `ObstacleScroller.cs` | Each obstacle **prefab** | Moves left, self-destructs |
| `WorldScroller.cs` | Ground tile **prefab** | Moves tile left |
| `ParallaxBackground.cs` | Background/midground layers | Looping parallax scroll |
| `UIManager.cs` | Canvas or UI GameObject | HUD + Game Over screen |

---

## Step-by-Step Unity Setup

### 1 — Create the Project
- Open Unity Hub → **New Project → 2D (URP or Built-in)**
- Name it `EndlessRunner`

### 2 — Import Scripts
Copy all `.cs` files into `Assets/Scripts/`.

### 3 — Create the Player
1. **GameObject → 2D Object → Sprite** → name it `Player`
2. Add components: `Rigidbody2D`, `BoxCollider2D`, `Animator`
3. Tag it **"Player"**
4. Attach `PlayerController.cs`
5. Create a child empty GameObject named `GroundCheck`, position it at the player's feet (e.g. y = -0.5)
6. In `PlayerController`, drag `GroundCheck` into the **Ground Check** field
7. Set **Ground Layer** to whatever layer your ground tiles are on (see step 4)

### 4 — Create a Ground Tile Prefab
1. **GameObject → 2D Object → Sprite** → name it `GroundTile`
2. Scale to roughly (10, 1, 1)
3. Add `BoxCollider2D`
4. Assign a **Layer** called `Ground`
5. Attach `WorldScroller.cs`
6. Drag into `Assets/Prefabs/` to make a prefab

### 5 — Create Obstacle Prefabs
1. **GameObject → 2D Object → Sprite** → name it e.g. `Cactus`
2. Add `BoxCollider2D` (adjust to fit sprite)
3. Tag it **"Obstacle"**
4. Attach `ObstacleScroller.cs`
5. Drag into Prefabs folder — repeat for more obstacle types

### 6 — Set Up GameManager
1. **GameObject → Create Empty** → name it `GameManager`
2. Attach `GameManager.cs`
3. Set **Start Speed** = 5, **Max Speed** = 20, **Speed Increase Rate** = 0.5

### 7 — Set Up GroundSpawner
1. **GameObject → Create Empty** → name it `Spawner`
2. Attach `GroundSpawner.cs`
3. Drag your `GroundTile` prefab into **Ground Tile Prefab**
4. Drag your obstacle prefabs into the **Obstacle Prefabs** array
5. Set **Tile Width** = 10 (match your ground tile scale)

### 8 — Parallax Background
1. Add 2–3 background sprites to the scene (sky, mountains, trees, etc.)
2. Attach `ParallaxBackground.cs` to each
3. Set **Parallax Factor**: sky = 0.1, mountains = 0.3, trees = 0.6
4. Set **Tile Width** to match the sprite width (use sprite renderer bounds)

### 9 — UI Setup
1. **GameObject → UI → Canvas**
2. Add a `TextMeshPro` for **Score** (top-left) and **High Score** (top-right)
3. Add a `Panel` named `GameOverPanel` with:
   - TextMeshPro: "GAME OVER"
   - TextMeshPro: Final Score
   - TextMeshPro: High Score
   - Button: "Restart"
4. Create an empty GameObject, attach `UIManager.cs`, and wire up all the UI references

### 10 — Camera
- Set Main Camera to **Orthographic**
- Position at roughly (0, 1, -10)
- The camera stays fixed; the world scrolls past

---

## Controls
| Input | Action |
|---|---|
| `Space` | Jump |
| `Left Mouse Click` | Jump (mobile-friendly) |

---

## Tips & Customization

- **Double jump**: Track a `jumpCount` int in `PlayerController`, allow jump when `jumpCount < 2`
- **Slide/Duck**: Add a crouch key that changes the collider size
- **Coins**: Add coin prefabs with `ObstacleScroller` + `OnTriggerEnter2D` to add score bonus
- **Sound**: Add `AudioSource` calls in `PlayerController.Jump()` and `Die()`
- **Animations**: Create an Animator with states: `Run`, `Jump`, `Fall`, `Die` — the controller already sets the trigger/bool parameters

---

## Scene Hierarchy (final)

```
Main Camera
GameManager
Spawner (GroundSpawner)
Player
  └── GroundCheck
Background_Sky    (ParallaxBackground 0.1)
Background_Mountains (ParallaxBackground 0.3)
Background_Trees  (ParallaxBackground 0.6)
Canvas
  ├── ScoreText
  ├── HighScoreText
  └── GameOverPanel
        ├── GameOverText
        ├── FinalScoreText
        ├── FinalHighScoreText
        └── RestartButton
UIManager
```
