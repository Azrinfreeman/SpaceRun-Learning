# Documentation validation

Reviewed on 2026-10-07 against `main` commit `64f868947e2a2bc7ee8e380e6cb28c21fd50a7cc`.

## Checks completed

- Confirmed Unity 6000.4.5f1 and the README package versions from project metadata.
- Confirmed two enabled build scenes, `Menu` and `Game`, and checked their paths against the repository tree.
- Confirmed the menu targets `Game`, and the menu, game manager, player, level managers, and progress scripts are referenced in their expected scenes.
- Checked the gameplay scene's three-health setting, Level 2 reward/penalty values of 15/5, and Level 3 values of 20/8.
- Reviewed grounded jump controls, instruction dismissal, collectible audio and progress, letter/word question managers, health, high-score storage, completion, and restart paths.
- Confirmed `activeInputHandler: 2` in project settings and documented legacy Input usage without assuming a device test.
- Checked relative documentation links, scene paths, and the final diff for whitespace errors. Changes are limited to Markdown; runtime files, scenes, assets, and package versions are preserved.

## Manual validation still needed

1. Open the complete project in Unity 6000.4.5f1, inspect imports and the Console, and start from `Menu`.
2. Test each level-selection route and instruction dismissal by timeout, click, button, and touch on the intended device.
3. Check jumping, ground detection, obstacles, health loss, invincibility, and game-over UI. Validate scrolling and spawn placement at different aspect ratios.
4. In Level 1, collect items and check score, delayed audio, effects, and progress.
5. In Levels 2 and 3, check spoken prompts, answer choices, correct/wrong feedback, penalties, missed items, question repetition, and insufficient content-pool handling.
6. Fill the progress slider, restart after completion and after death, and confirm progress, health, spawners, and question state reset consistently. Test rapid successive pickups because progress starts an animation coroutine per pickup.
7. Return home, change modes, and verify high-score persistence after restarting the application. Build for the intended target and repeat the relevant checks.

The older script README described assembling a generic runner from scratch. It was replaced with a guide to this repository's current components, while the root README provides the full-project setup path.

No Unity Editor session, automated Unity tests, device run, or release build was performed. This documentation review does not certify runtime correctness, asset licensing, repository history safety, or suitability for public release.
