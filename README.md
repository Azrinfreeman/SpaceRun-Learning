# SpaceRun

A Unity 2D space runner that combines jumping and obstacle avoidance with audio-based letter and word recognition. Three selectable learning modes share one gameplay scene, with collectible rewards, health, progress toward completion, and a locally saved high score.

This repository contains the Unity project source. The overview below was checked against scripts and serialized scene settings; a fresh Editor playthrough or build has not been verified. See [validation notes](docs/VALIDATION.md).

## Learning modes

| Mode | Activity | Feedback and scoring |
| --- | --- | --- |
| Level 1 | Jump to collect items while avoiding obstacles | Collectibles add their configured score, increase progress, and can play their own audio after a pickup sound |
| Level 2 | Listen to a letter prompt and collect the matching answer | The scene configures two wrong alternatives, +15 for a correct answer, and a 5-point penalty for an incorrect answer |
| Level 3 | Listen to a word prompt and collect the matching answer | The spawner requests one correct word and two alternatives; the scene configures +20 for correct answers and an 8-point penalty for incorrect answers |

The level managers coordinate spoken prompts, collectible spawning, feedback, and subsequent questions. Incorrect Level 2 answers request a new random question; incorrect Level 3 answers can replay the current prompt. The number of available choices depends on the configured content pool.

## Gameplay and engineering highlights

- **Runner movement:** grounded jumping uses `Rigidbody2D`, with animation parameters and optional jetpack particles and sound.
- **Scrolling world:** ground recycling, obstacle scrolling, parallax backgrounds, and collectible clusters support the runner loop.
- **Health and feedback:** the gameplay scene sets three health points, with temporary invincibility after damage and a heart display.
- **Score and completion:** score grows with running speed and pickup rewards; correct collectibles increase an animated progress slider that triggers completion when full.
- **Restart handling:** the game manager resets health, player state, spawners, question modes, and progress when restarting.
- **Local persistence:** the high score uses Unity `PlayerPrefs`.

## Open the project

1. Clone or download the complete repository, preserving Unity `.meta` files and the asset folders.
2. Add the repository root in Unity Hub and open it with **Unity 6000.4.5f1**, recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
3. Allow asset imports and dependency restoration to finish. [Packages/manifest.json](Packages/manifest.json) records Universal Render Pipeline 17.4.0, Input System 1.19.0, Unity UI 2.0.0, and Timeline 1.8.12.
4. Open `Assets/Scenes/Menu.unity`, enter Play mode, and choose a level. The menu's serialized scene target is `Game`.
5. Dismiss the instructions by clicking/tapping, using the start button, or waiting for the countdown. The instruction controller restores time and calls `StartGame()`.
6. For a build, retain the two enabled scenes in [EditorBuildSettings.asset](ProjectSettings/EditorBuildSettings.asset): `Menu` followed by `Game`. All three modes run within `Game`.

Both input backends are enabled in the recorded project settings. The reviewed gameplay controller uses the legacy `Input` API despite the Input System package being present. Start with the recorded editor version; upgrades and device compatibility have not been tested.

## Controls

| Input | Action |
| --- | --- |
| Space or left mouse click | Jump while grounded |
| Click/tap during instructions | Dismiss instructions and start the selected mode |
| Menu buttons | Select Level 1, 2, or 3, or exit |
| Gameplay UI buttons | Restart or return home through the game manager |

Mobile jumping must be checked on a device; this review confirmed the mouse/keyboard code paths and the instruction panel's explicit touch detection, without a device test.

## Source guide

See the [script guide](Assets/scripts/README.md) for the main components and their responsibilities. Important entry points include [Mainmenumanager.cs](Assets/scripts/Mainmenumanager.cs), [GameManager.cs](Assets/scripts/GameManager.cs), [PlayerController.cs](Assets/scripts/PlayerController.cs), [Level2Manager.cs](Assets/scripts/Level2Manager.cs), and [Level3Manager.cs](Assets/scripts/Level3Manager.cs).

## Project status and reuse

The project remains under development. Source and scene checks support the documented features, but runtime behavior, content coverage, audio timing, restart reliability, and a target-device build still need validation. No learning-effectiveness assessment is claimed.

Artwork, audio, fonts, and imported packages may carry separate reuse terms. This documentation does not add a repository-wide license or establish redistribution rights for those assets. Review their original licenses before distributing a build or reusing content.
