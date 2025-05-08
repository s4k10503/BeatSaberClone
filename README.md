# BeatSaber Clone

This repository aims to clone BeatSaber in Unity.
I am trying to reproduce the VR rhythm game part, which is an in game of this work.

![Image](https://github.com/user-attachments/assets/8991b111-c1d5-4c9c-9e19-a51528d85bec)  
[Video](https://github.com/user-attachments/assets/f1158c08-2bfe-42d6-aa65-38c49aa1b9ba)  

## Requirements

Unity 2022.3.11f1 LTS or later

- This project has been developed on Windows and cannot be executed on other OS.
- It has been confirmed that it builds and starts for Meta Quest 3.

## How to Work

- Clone this Repository and open it in the Unity editor (ignore the error)
- Open the "GameScene"

### Extenject

1. Get Extenject and DOTween in the Asset Store (if you haven't already)
2. Select "My Assets" in the package manager and apply Extenject

### DOTween

1. Get Extenject and DOTween in the Asset Store (if you haven't already)
2. Select "My Assets" in the package manager and apply DOTween
3. Please set up according to the setup manager

### EzySlice

1. Download EzySlice and Expanded Assets/Plugins
2. Create assembly definition file in the Ezyslice directory

### TextMeshPro

1. When you start the project, the import window is dynamically displayed
2. Import all the suggestions

## Architecture

Onion architecture + MVP pattern

- Loosely coupled architecture design
  - Event driven processing by UniRx
  - Asynchronous processing of single threads and multi threads by UniTask
  - Dependency injection by Zenject

## Third Party Assets

- NugetForUnity
  - <https://github.com/GlitchEnzo/NuGetForUnity>

- NSubstitute
  - <https://github.com/nsubstitute/NSubstitute>

- EzySlice
  - <https://github.com/DavidArayan/ezy-slice>

- UniRx
  - <https://github.com/neuecc/UniRx>

- UniTask
  - <https://github.com/Cysharp/UniTask>

- Extenject
  - <https://assetstore.unity.com/packages/tools/utilities/extenject-dependency-injection-ioc-157735>

- DOTween
  - <https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676>

- MixedRealityToolkit
  - <https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity?tab=BSD-3-Clause-1-ov-file#readme>

## References

- <https://www.youtube.com/watch?v=E2rktIcLJwo>
- <https://www.youtube.com/watch?v=ozByDujKyDc>
- <https://note.com/logic_magic/n/n47e91a1e65bb>
