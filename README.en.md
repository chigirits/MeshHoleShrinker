# MeshHoleShrinker

A Unity editor extension that adds a shape key to shrink a hole in a mesh and save it as a new mesh.

This is useful in Unity for filling in the gaps at the joint between your avatar's head and the other model's body.

![demo](https://repository-images.githubusercontent.com/295406109/a2eb0680-f6d5-11ea-9672-da464eeef28f)

## Required Environment

Unity 2018.4 or later

## How to Import

### By .unitypackage file

Download the latest version of `MeshHoleShrinker-vX.X.X.unitypackage` from [Releases page](https://github.com/chigirits/MeshHoleShrinker/releases) and import it into your Unity project.

### By Unity Package Manager

1. Open `Packages/manifest.json` in your Unity project with a text editor.
2. Add the following key-value to the object `dependencies`.
   
   ```
   "com.github.chigirits.MeshHoleShrinker": "https://github.com/chigirits/MeshHoleShrinker.git",
   ```

In this case, look for the prefabs and presets shown in the following descriptions from `Packages/MeshHoleShrinker/Assets/Chigiri/MeshHoleShrinker/...`, not from `Assets/Chigiri/MeshHoleShrinker/...`.

## Usage

1. Place an avatar in the scene and hide all but the head.
2. Selecting "Chigiri/Create MeshHoleShrinker" from the menu places a MeshHoleShrinker in the top level of the hierarchy. Some [presets](#Presets) for avatars sold at Booth are available.
