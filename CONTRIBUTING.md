# Contributing

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

For an existing branch, use `git switch` without `-c`. Work primarily in your directories listed in docs/TEAM_TASKS.md. Discuss major Core changes first. Thai coordinates Main.unity; other members should test scripts and prefabs in their own separately named test scenes. Commit assets together with their .meta files. Move assets in Unity to preserve GUIDs. Do not commit caches or unfinished features to main.

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
