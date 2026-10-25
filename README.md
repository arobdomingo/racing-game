# Physics-Driven Racing Game

An F1-inspired vehicle physics prototype built in **Unity 6 and C#**. This is a handling and drivetrain experiment, not a finished racing game.

## Implemented
- Four WheelColliders with Rigidbody-based movement and rear-wheel drive.
- Configurable torque curve, eight gear ratios, and final-drive ratio.
- Keyboard steering, throttle, braking and manual gear changes.
- Camera script and Unity test scenes.

## Tech
Unity 6000.1.0f1, C#, Unity physics.

## Run
Clone this repository, open it with **Unity Hub** using Unity **6000.1.0f1**, open `Assets/Scenes/SampleScene.unity` or the F1 demo scene, and press Play.

In the current `Movement.cs` script: **P** throttle, **O** brake, **A/D** steering, **comma/period** downshift/upshift.

## Focus
Vehicle dynamics, force calculations, modular controls, and iterative physics tuning. **Status: prototype / in development.**
