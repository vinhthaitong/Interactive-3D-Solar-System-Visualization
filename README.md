# Interactive 3D Solar System Visualization

CS 4361 Computer Graphics — Team 3. Unity, C#, and GitHub.

An educational solar system that will let users explore the Sun and eight major planets from a first-person view. This repository provides only the shared foundation; the application features remain assigned to the team.

## Planned features

- Sun and Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune in order.
- Planet revolution, axial rotation, and visible orbital paths.
- First-person camera movement, zoom, and planet selection.
- Selected planet name, distance from Sun, distance from Earth, diameter, moon count, and description.
- Simple lighting, recognizable colors, materials, and optional textures.

The final application should start automatically with preset sizes and speeds. No advanced physics, multiplayer, extra settings, or third-party packages are planned.

## Project structure

```text
Assets/
  Scenes/                     # Main.unity generated in Unity
  Scripts/
    Core/                     # Four shared components
    Editor/                   # CreateStarterScene menu command
    SceneSetup/               # Thai
    UI/                       # Denny
    CameraControls/           # Jose
    Interaction/              # Jose
    Motion/                   # Shivansh
  Prefabs/CelestialBodies/     # Thai
  Prefabs/UI/                  # Denny
  Materials/                  # Thai
  Textures/                   # Thai
  Data/                       # Denny
Packages/                     # Unity-generated package files pending
ProjectSettings/              # Unity-generated settings pending
.github/pull_request_template.md
.gitignore
README.md                     # Setup, tasks, collaboration, and progress
```

Empty asset folders contain `.gitkeep` so Git preserves them. This README is the only maintained project documentation file; the pull request template remains for reviews.

## Setup on Windows and macOS

**Team Editor version: Unity 6000.6.8f1 (Unity 6.6).** This exact Editor was detected on Thai's Mac. All four members should install the same full version, including the patch number.

### 1. Install the tools

1. Install Git and Unity Hub on your computer.
2. Sign in to Unity Hub and activate the license appropriate for your account when prompted.
3. Under **Installs → Install Editor**, install **6000.6.8f1**. If it is absent from the current list, locate that exact version in the [official Unity download archive](https://unity.com/releases/editor/archive) and use its Unity Hub installation link.
4. Windows users install the Windows Editor. Mac users select Apple Silicon or Intel to match their Mac. The Editor version number must match across operating systems.
5. Android, iOS, and WebGL modules are not needed for the starter project. Add platform build support later only if the team needs to export to that platform.

Run Git commands in **Terminal on macOS** or **Git Bash on Windows**. Start in the folder where you want the repository stored:

```bash
git clone https://github.com/vinhthaitong/Interactive-3D-Solar-System-Visualization.git
cd Interactive-3D-Solar-System-Visualization
```

If you already have this repository, use that existing folder instead of cloning again. Thai's current checkout is named `CS 4361-Project`; that folder name is fine.

### 2. First initialization — Thai only

The repository currently contains source and a scene generator. `Main.unity`, `Packages/manifest.json`, and Unity-generated project settings have not been generated yet. Thai completes these steps once before teammates begin feature work:

1. In Unity Hub create a **separate temporary project** using **6000.6.8f1** and the **3D Built-In Render Pipeline** template. Open it once, wait for initialization, then close the Editor.
2. Copy that temporary project's generated `Packages/manifest.json`, `Packages/packages-lock.json` (if generated), and the contents of `ProjectSettings/` into the corresponding directories in this repository. Preserve the repository's existing `Assets/` and README. Do not copy `Library/`, `Temp/`, or template scenes.
3. In Unity Hub select **Projects → Add → Add project from disk** and choose the repository root containing `Assets/`, `Packages/`, and `ProjectSettings/`.
4. Open it with **6000.6.8f1** and wait for import and compilation. Unity creates genuine `.meta` files.
5. Run **Solar System → Create Starter Scene** from the Editor menu. The command creates `Assets/Scenes/Main.unity`, connects the shared references, enables Force Text and Visible Meta Files, and sets Main as the startup build scene. It refuses to overwrite an existing Main scene.
6. Open `Assets/Scenes/Main.unity`, press **Play**, and complete the verification checklist below. Stop Play before making persistent scene edits.
7. Verify `ProjectSettings/ProjectVersion.txt` records `6000.6.8f1`. Commit the generated scene, `.meta` files, package files, and project settings to `main` as the remaining foundation initialization:

```bash
git status
git add .
git diff --cached --check
git diff --cached --stat
git commit -m "chore: generate Unity project settings and starter scene"
git push origin main
```

Do not commit if Unity still reports errors. Keep this README's progress section accurate after verification.

### 3. Open and run — all members after initialization

1. Commit any local work before pulling updates. On `main`, run `git pull origin main`.
2. Install the exact Editor version recorded in `ProjectSettings/ProjectVersion.txt`.
3. In Unity Hub add the repository root as a project and open it. Each computer generates its own `Library/`; do not copy it from a teammate.
4. Wait for import and compilation, then double-click `Assets/Scenes/Main.unity` and press **Play**.
5. The starter should display nine stationary spheres: the Sun and eight planets. Sizes and spacing are illustrative. Camera movement, selection, motion, UI, and recognizable materials remain team tasks.

### Verification checklist

These checks are **pending**, not recorded as passed:

- [ ] Console has no compilation errors or runtime exceptions.
- [ ] SolarSystem contains Sun and Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune in order.
- [ ] Each planet has a collider and matching CelestialBody; the Sun has no PlanetId.
- [ ] SolarSystemReferences holds the Sun, eight planets, Main Camera, and SelectionService.
- [ ] The Game view shows all placeholders with a camera and directional light.
- [ ] Selecting a different body through SelectPlanet updates CurrentSelection and raises SelectionChanged; repeating the selection does not; null clears it.
- [ ] Running the scene generator again refuses to overwrite Main.unity.
- [ ] Project settings use Force Text serialization and Visible Meta Files; Main is enabled as the startup scene in build settings/profiles.
- [ ] Git tracks `.meta` files and excludes `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, and `UserSettings/`.

## Shared components

- `PlanetId`: eight planet identifiers in orbital order; excludes the Sun.
- `CelestialBody`: identifies each planet and exposes BodyTransform.
- `SelectionService`: stores CurrentSelection; SelectPlanet publishes SelectionChanged on changes, including null to clear.
- `SolarSystemReferences`: Inspector references to Sun, planets, camera, and selection service.

Thai creates bodies and references. Motion receives Transforms. Jose announces selections through SelectionService. Denny listens and updates UI. Keep shared contracts small and discuss major changes.


## Team responsibilities

### Thai Tong

Branch: `feature/thai-scene-visuals`

Owned directories: Assets/Scripts/SceneSetup/, Assets/Scenes/, Assets/Materials/, Assets/Textures/, Assets/Prefabs/CelestialBodies/

- [ ] Sun and planets
- [ ] Layout
- [ ] Lighting
- [ ] Materials
- [ ] Textures
- [ ] Celestial prefabs
- [ ] Coordinate Main.unity and final integration

### Denny Dat Tran

Branch: `feature/denny-ui`

Owned directories: Assets/Scripts/UI/, Assets/Data/, Assets/Prefabs/UI/

- [ ] Planet data
- [ ] Information panel
- [ ] Show/hide behavior
- [ ] Selection event subscription

### Jose Beltran Gonzalez

Branch: `feature/jose-camera-selection`

Owned directories: Assets/Scripts/CameraControls/, Assets/Scripts/Interaction/

- [ ] First-person navigation
- [ ] Zoom
- [ ] Raycast planet selection
- [ ] SelectionService notification

### Shivansh Vaidhyanathan

Branch: `feature/shivansh-motion`

Owned directories: Assets/Scripts/Motion/

- [ ] Revolution
- [ ] Axial rotation
- [ ] Reusable movement components
- [ ] Visible orbital paths

### Shared integration

- [ ] All members install Unity 6000.6.8f1 and verify the generated starter scene.
- [ ] Discuss major Core changes before implementation.
- [ ] Review pull requests and test components independently.
- [ ] Thai coordinates scene integration; avoid simultaneous scene edits.
- [ ] Test the combined application and fix integration issues.
- [ ] Prepare the progress report, final report, presentation, and demonstration.

The team changed from five members to four after Zachary Sherrell dropped the course. Responsibilities were redistributed among the four active members; no tasks are assigned to the former member.

### Component contract

Thai supplies colliders, CelestialBody components, and SolarSystemReferences. Shivansh receives Sun and planet Transforms. Jose calls SelectPlanet on the shared SelectionService. Denny subscribes to SelectionChanged, handles null to hide the panel, and unsubscribes when disabled. On enabling UI, read CurrentSelection as well, so a prior selection is displayed. Each system owns its own state. No feature implementations are included in this foundation.

## Git collaboration

Clone and update main:

```bash
git clone https://github.com/vinhthaitong/Interactive-3D-Solar-System-Visualization.git
cd Interactive-3D-Solar-System-Visualization
git switch main
git pull origin main
```

Create your assigned branch once (replace the example with your actual assigned name):

```bash
git switch -c feature/your-branch-name
```

| Member | Branch |
| --- | --- |
| Thai | feature/thai-scene-visuals |
| Denny | feature/denny-ui |
| Jose | feature/jose-camera-selection |
| Shivansh | feature/shivansh-motion |

For an existing branch, use `git switch` without `-c`. Work primarily in your directories listed in the team responsibilities below. Discuss major Core changes first. Thai coordinates Main.unity; other members should test scripts and prefabs in their own separately named test scenes. Commit assets together with their .meta files. Move assets in Unity to preserve GUIDs. Do not commit caches or unfinished features to main.

Review and commit work:

```bash
git status
git add .
git diff --cached
git commit -m "feat: describe the implemented feature"
git push -u origin feature/your-branch-name
```

To bring later main updates into your existing feature branch, first commit your work, then run these commands while on your feature branch:

```bash
git fetch origin
git merge origin/main
```

Resolve conflicts together with the affected owner, stage resolved files, and commit the merge. If you need to stop a conflicted merge, use `git merge --abort`. Do not discard teammate changes, force-push, or rewrite shared history. Ask Thai to resolve scene conflicts with the owners.

On GitHub, open **Pull requests → New pull request**, select `main` as the base and your feature branch as the compare branch. Fill in the template, request teammate review, and merge after review and testing. Update local main afterward. No feature branches were created during initialization.

## Project progress notes

Course Project Progress report due: **November 9, 2026**.

### Completed work

Repository foundation: shared Core source, starter-scene Editor command, Git ignore rules, pull request template, and this consolidated README. Unity-generated scene and project settings are pending.

### Work in progress

Fill in actual work and evidence; do not mark planned features complete.

### Problems encountered

Unity was unavailable during foundation creation. Unity 6000.6.8f1 is now installed on Thai’s Mac, but compilation and runtime verification are still pending.

### Changes from the original plan

Team changed from five to four after Zachary Sherrell dropped the course. Scene/visuals: Thai; UI/data: Denny; camera/selection: Jose; motion/orbits: Shivansh.

### Upcoming tasks

Initialize Unity with 6000.6.8f1, generate and verify Main.unity, then implement assigned features on separate branches.

### Individual contributions

- Thai Tong: [record completed work and commit/PR]
- Denny Dat Tran: [record completed work and commit/PR]
- Jose Beltran Gonzalez: [record completed work and commit/PR]
- Shivansh Vaidhyanathan: [record completed work and commit/PR]

### Testing evidence

[Date, Editor version, tester, steps, observed result, known issues]
