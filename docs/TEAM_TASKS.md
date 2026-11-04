# Team 3 responsibilities

## Thai Tong

Branch: `feature/thai-scene-visuals`

Owned directories: Assets/Scripts/SceneSetup/, Assets/Scenes/, Assets/Materials/, Assets/Textures/, Assets/Prefabs/CelestialBodies/

- [ ] Sun and planets
- [ ] Layout
- [ ] Lighting
- [ ] Materials
- [ ] Textures
- [ ] Celestial prefabs
- [ ] Coordinate main.unity and final integration

## Denny Dat Tran

Branch: `feature/denny-ui`

Owned directories: Assets/Scripts/UI/, Assets/Data/, Assets/Prefabs/UI/

- [ ] Planet data
- [ ] Information panel
- [ ] Show/hide behavior
- [ ] Selection event subscription

## Jose Beltran Gonzalez

Branch: `feature/jose-camera-selection`

Owned directories: Assets/Scripts/CameraControls/, Assets/Scripts/Interaction/

- [ ] First-person navigation
- [ ] Zoom
- [ ] Raycast planet selection
- [ ] Selectionservice notification

## Shivansh Vaidhyanathan

Branch: `feature/shivansh-motion`

Owned directories: Assets/Scripts/Motion/

- [ ] Revolution
- [ ] Axial rotation
- [ ] Reusable movement components
- [ ] Visible orbital paths

## Shared integration

- [ ] Select a single Unity version and verify the generated starter scene.
- [ ] Discuss major Core changes before implementation.
- [ ] Review pull requests and test components independently.
- [ ] Thai coordinates scene integration; avoid simultaneous scene edits.
- [ ] Test the combined application and fix integration issues.
- [ ] Prepare the progress report, final report, presentation, and demonstration.

The team changed from five members to four after Zachary Sherrell dropped the course. Responsibilities were redistributed among the four active members; no tasks are assigned to the former member.

## Component contract

Thai supplies colliders, CelestialBody components, and SolarSystemReferences. Shivansh receives Sun and planet Transforms. Jose calls SelectPlanet on the shared SelectionService. Denny subscribes to SelectionChanged, handles null to hide the panel, and unsubscribes when disabled. On enabling UI, read CurrentSelection as well, so a prior selection is displayed. Each system owns its own state. No feature implementations are included in this foundation.
