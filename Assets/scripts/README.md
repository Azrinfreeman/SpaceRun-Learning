# SpaceRun script guide

Open the complete project from the repository root using the [main setup guide](../../README.md). These scripts form the existing game; creating a new generic runner project is not required.

| Area | Components | Responsibility |
| --- | --- | --- |
| Menu and startup | `Mainmenumanager.cs`, `InstrutionPanel.cs` | Select one of three modes, load Game, show instructions, and start gameplay |
| Run state | `GameManager.cs`, `UIManager.cs` | Speed, score, high score, completion, game over, restart, and home navigation |
| Player | `PlayerController.cs`, `Playerhealth.cs`, `PlayerJetpack.cs` | Grounded jumping, damage and invincibility, animation, particles, and audio |
| World | `GroundSpawner.cs`, `WorldScroller.cs`, `ObstacleScroller.cs`, `ParallaxBackground.cs` | Recycle terrain, scroll obstacles, and loop backgrounds |
| Obstacle contact | `ObstacleHit.cs`, `ObstacleEffect.cs` | Damage and obstacle feedback |
| Level 1 items | `CollectibleSpawner.cs`, `CollectibleItem.cs` | Spawn collectible clusters, reward pickups, and play item audio |
| Level 2 letters | `Level2Manager.cs`, `Level2CollectSpawner.cs`, `Level2CollectibleItem.cs` | Spoken letter prompts, answer spawning, feedback, and question progression |
| Level 3 words | `Level3Manager.cs`, `Level3CollectibleSpawner.cs`, `Level3CollectibleItem.cs` | Spoken word prompts and answer choices |
| Progress and effects | `ProgressSlider.cs`, `CollectibleEffect.cs`, `ColectibleSoundPlayer.cs`, `sfxScript.cs` | Animate progress and play pickup feedback |

Some filenames differ from their class names: `Mainmenumanager.cs` defines `MainMenuManager`, `Playerhealth.cs` defines `PlayerHealth`, and `InstrutionPanel.cs` defines `InstructionPanel`. These existing names are retained. `Leveldata.cs` defines a QuestionData ScriptableObject; the reviewed level managers use their own LetterEntry/WordEntry arrays.

The game uses Space or left mouse click for grounded jumps. The instruction panel also checks touch input. Inspector references, content pools, UI callbacks, and device input require a playthrough; see [validation notes](../../docs/VALIDATION.md).
