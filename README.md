# Unity Physics 2D Plugin
 Unity Physics extension for adding pseudo 2D physics functionality

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JP.md)

## Overview

The Unity Physics 2D Plugin is a library for implementing pseudo 2D physics behavior using Unity Physics.

Currently, Unity Physics does not support 2D, and if you want to use it in a 2D project, you need to use 3D Rigidbody component.

The Unity Physics 2D Plugin simulates 2D physics behavior using 3D colliders with the Z-axis fixed, providing support for `Rigidbody2D` and several 2D colliders. It converts `Rigidbody2D` and `Collider2D` components placed within a SubScene into components compatible with Unity Physics.

### Requirements

* Unity 2022.3 or higher
* Entities 1.0.0 or higher
* Unity Physics 1.0.0 or higher

### Installation

1. Open the Package Manager from Window > Package Manager.
2. Click the "+" button > Add package from git URL.
3. Enter the following URL:

```
https://github.com/AnnulusGames/UnityPhysics2DPlugin.git?path=Assets/UnityPhysics2DPlugin
```

Alternatively, open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.annulusgames.unity-physics-2d-plugin": "https://github.com/AnnulusGames/UnityPhysics2DPlugin.git?path=Assets/UnityPhysics2DPlugin"
    }
}
```

## Basic Usage

By installing the Unity Physics 2D Plugin, `Rigidbody2D` and supported `Collider2D` components within a SubScene are converted into a set of compatible components.

<img src="https://github.com/AnnulusGames/UnityPhysics2DPlugin/blob/main/Assets/UnityPhysics2DPlugin/Documentation~/img1.png" width="800">

Friction and bounciness values assigned to `Rigidbody2D` and colliders are reflected from the associated `PhysicsMaterial2D` assets. Regarding the CollisionFilter, it applies the settings of Physics2D's `Layer Collision Matrix`.

## Available Colliders

The Unity Physics 2D Plugin currently supports `BoxCollider2D`, `CircleCollider2D`, and `CapsuleCollider2D`. If you want to create complex-shaped colliders, you can combine these to create compound colliders.

## Physics2DTag

Entities created for 2D have a `Physics2DTag` component added. This allows you to distinguish between 2D and 3D physics bodies when querying.

## Mechanism

Physics bodies created with the Unity Physics 2D Plugin operate independently in a separate `Physics2DSystemGroup` from the regular ones. The `PhysicsWorldIndex` value of the Entity is set to 10, ensuring no interference with default colliders.

The Physics2DSystemGroup adds its custom systems before and after the regular Unity Physics systems. These systems temporarily set the Z-axis position and X/Y-axis rotation of the LocalTransform to 0 only during the simulation of physical behavior. Additionally, the center of mass position, Z-axis velocity, and X/Y-axis rotational velocity are all set to 0.

## Limitations

* 2D collision queries (e.g., Raycast, Overlap) are not supported (though you can use standard 3D collision queries since the actual colliders are created in 3D).
* Due to the addition of custom systems, the simulation of the Unity Physics 2D Plugin may not be deterministic.

## License

[MIT License](LICENSE)