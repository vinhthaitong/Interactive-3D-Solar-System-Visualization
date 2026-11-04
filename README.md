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

## Team

| Member | Responsibility |
| --- | --- |
| Thai Tong | Scene, Sun/planets, visuals, prefabs, final scene integration |
| Denny Dat Tran | Planet data and information UI |
| Jose Beltran Gonzalez | First-person camera, zoom, raycast selection |
| Shivansh Vaidhyanathan | Revolution, rotation, visible orbits |

See [task ownership and remaining work](docs/TEAM_TASKS.md).

## Structure

```text
Assets/
  Scenes/                      # Main.unity generated in Unity
  Scripts/
    Core/                      # PlanetId, CelestialBody, SelectionService, SolarSystemReferences
    SceneSetup/                # Thai
    UI/                        # Denny
    CameraControls/            # Jose
    Interaction/               # Jose
    Motion/                    # Shivansh
    Editor/                    # CreateStarterScene command
  Prefabs/CelestialBodies/      # Thai
  Prefabs/UI/                   # Denny
  Materials/                    # Thai
  Textures/                     # Thai
  Data/                         # Denny
Packages/                       # Unity-generated manifest pending
ProjectSettings/                # Unity-generated settings pending
.github/pull_request_template.md
docs/TEAM_TASKS.md
docs/SETUP.md
docs/PROGRESS_NOTES.md
.gitignore
README.md
CONTRIBUTING.md
```

## Open and play

Unity was unavailable during setup. The team must select one Editor version and complete [first initialization](docs/SETUP.md), which uses Unity-generated package/settings files. Then add the repository root in Unity Hub, open it, and run **Solar System → Create Starter Scene**. Open Assets/Scenes/Main.unity and press Play. The starter displays stationary placeholder spheres; team features are not implemented. Unity compile/runtime verification is pending.

## Shared components

- `PlanetId`: eight planet identifiers in orbital order; excludes the Sun.
- `CelestialBody`: identifies each planet and exposes BodyTransform.
- `SelectionService`: stores CurrentSelection; SelectPlanet publishes SelectionChanged on changes, including null to clear.
- `SolarSystemReferences`: Inspector references to Sun, planets, camera, and selection service.

Thai creates bodies and references. Motion receives Transforms. Jose announces selections through SelectionService. Denny listens and updates UI. Keep shared contracts small and discuss major changes.

## Contribute

Follow [CONTRIBUTING.md](CONTRIBUTING.md). Create your assigned branch from updated main, edit your own directories, test, and open a pull request for review. Thai coordinates Main.unity edits. Commit Unity .meta files with their assets; never commit caches or force-push shared history.
