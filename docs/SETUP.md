# Setup and validation

## Editor version

No Unity Editor was detected under /Applications during initialization. No Unity version has been selected or tested. The team must agree on one supported Editor version available in Unity Hub before development. Install exactly that same version through Hub → Installs → Install Editor. The teammate who initializes the project must commit Unity's generated ProjectSettings/ProjectVersion.txt; everyone else installs that recorded version. Prefer a minimal built-in 3D template to avoid render-pipeline dependencies.

## First initialization (Thai)

1. Clone using the commands in CONTRIBUTING.md. This repository is a foundation awaiting Unity initialization, not yet an Editor-generated project.
2. In Unity Hub create a temporary, separate project using the agreed Editor and minimal built-in 3D template. Open it once, then close Unity.
3. Copy the generated Packages/manifest.json, Packages/packages-lock.json (if generated), and the contents of ProjectSettings/ into this repository's corresponding directories. Preserve existing Assets and documentation. Do not copy Library, Temp, or template scenes over this repository.
4. In Hub choose Add project from disk and select this repository root (the directory containing Assets, Packages, and ProjectSettings). Open it with the agreed version. Unity generates genuine .meta files on import.
5. Wait for compilation. Run **Solar System → Create Starter Scene**. This sets Visible Meta Files and Force Text, generates Assets/Scenes/Main.unity, and adds it as the startup build scene. It refuses to overwrite an existing Main.unity; canceling an unsaved-scene prompt also leaves the scene alone.
6. Verify Edit → Project Settings → Editor shows Force Text, and Version Control mode is Visible Meta Files (location varies by version). Check build settings list Main as the enabled startup scene. Commit all generated .meta files, Main.unity, package files, and ProjectSettings on main as the remaining foundation initialization, before feature work begins.

## Open and run after initialization

Clone the repository, install the version recorded in ProjectVersion.txt, add the repository root in Hub, and open it. Double-click Assets/Scenes/Main.unity, then press Play. Nine stationary spheres should appear automatically in the Game view; the camera frames the Sun and eight planets. Placeholder spacing and sizes are illustrative, not to scale. Movement, interaction, colors, and UI are intentionally pending.

## Manual acceptance checks

- Console has no compilation errors or runtime exceptions.
- SolarSystem contains Sun and Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune in order.
- Each planet has a sphere collider and matching CelestialBody. Sun has no PlanetId.
- SolarSystemReferences contains the Sun, eight planets, Main Camera, and SelectionService.
- Main Camera and directional light exist; Game view displays all placeholders.
- Calling SelectPlanet with a different body updates CurrentSelection and raises SelectionChanged; repeating the same value does not. Selecting null clears selection.
- Scene generation a second time refuses to overwrite Main.unity.
- Visible Meta Files and Force Text are configured; git status excludes Unity caches and includes .meta files.

These Unity compilation, scene, and behavior checks were not run in the initialization environment. Record the agreed Editor version and observed results here after running them.
