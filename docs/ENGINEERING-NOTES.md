# Engineering a 3D Racing Game: C#, Unity, Physics, and Software Architecture

Working textbook notes — milestone 1: establishing a driving baseline.

## Sampling input and advancing physics

Unity may render several frames between physics steps, or advance several physics
steps before a rendered frame. Update samples held keys and shift edges;
FixedUpdate applies steering, drive and brake torque every 0.02 simulated seconds.
The latest held input is a small command buffer. Gear edges update a bounded gear
index immediately so a brief keypress is not lost between physics ticks.

For a pedal value p and target q, MoveTowards changes p by at most rate * dt.
At rate 2 per second, a pedal reaches full input in 0.5 seconds (25 physics ticks).
This replaces 0.01 per rendered frame: previously 30 FPS took 3.33 seconds and
120 FPS took 0.83 seconds. Steering follows the same rule in degrees per second
and returns to exactly zero when neither direction is pressed. A and D cancel.

The legacy P/O and A/D controls remain to avoid simultaneously changing the input
scheme and handling model. Input sampling is separated as a method boundary, but
not yet a reusable input-provider component. A later input abstraction can supply
the same normalized targets for a gamepad or race countdown lockout.

## Transmission approximation

Let r be the selected gear ratio times final drive. Then engineRPM = wheelRPM * r,
and axle torque = engineTorque * r. Split rear axle torque equally between two
rear WheelColliders; do not apply full axle torque to both.

Example: at peak engine torque 300 Nm, first gear 3.1 and final drive 4.9,
axle torque is 4557 Nm, or 2278.5 Nm per rear wheel at full throttle. The curve
scales that peak by normalized engine RPM. These are torques, not forces: tire
force also depends on wheel radius, available grip and the physics solver.

An idle floor of 2000 RPM prevents a curve starting at zero from trapping the car
at rest. Absolute driven-wheel RPM avoids negative curve inputs while rolling
backward. Above 15000 coupled RPM, drive torque is cut until RPM falls or the
driver upshifts. This is an inexpensive gameplay approximation of clutch slip and
a rev limiter. It does not model engine inertia, a clutch, reverse, neutral,
engine braking or over-rev protection on downshift. Tire slip can trip the limiter.

The brakeTorque field now means the whole-car budget: each front wheel receives
0.35 * budget and each rear wheel 0.15 * budget. Sum = 1.0. The original sum was
2.0, so existing brake values may need increasing after a stopping-distance test.

## Presentation and state ownership

HUD-readable speed, gear and input values are properties with private setters;
only Movement advances them. Configuration fields keep their serialized names,
which preserves prefab and scene references. Former runtime fields were zeroed
(or gear set to one) in the checked-in prefab; they no longer persist runtime state.
Awake checks required physics and HUD references with explicit error messages.

The camera reads the assigned controller transform instead of searching a tagged
mesh. It runs in LateUpdate after simulation/render updates, with Rigidbody
interpolation reducing visible 50 Hz stepping. Its offset uses vehicle yaw so roll
and pitch do not overturn the camera. Model forward direction needs Editor review.

Camera blend = 1 - exp(-sharpness * dt). Over one second, remaining error is
exp(-sharpness), regardless of how that second is divided into frames for a fixed
target. At sharpness 6, about 0.25% remains. A moving target still produces lag.
The camera follows even at zero speed. Collision avoidance is future polish.

## Independent acceptance test

Open with Unity 6000.1.0f1, load Assets/F1/F1 Demo.unity, clear the Console and
enter Play Mode. Ensure the Game view has keyboard focus.

1. Hold P from rest: speed rises; rear wheels drive, front wheels do not.
2. Release P: throttle reaches zero in approximately 0.5 s. Hold O to stop.
3. Hold A/D, release: steering centers without W; A+D centers too.
4. Tap comma/period repeatedly: gear remains between 1 and 8. Hold a shift key:
   it shifts only once per press. Drive until redline and verify torque cutoff.
5. At rest and while turning: verify a stable upright chase view. If it is in
   front of the model, inspect its axis and adjust followOffset in the Inspector.
6. Compare a five-second launch and steering response at 30/60/120 FPS with VSync
   disabled. Expect similar response; physics is not bitwise deterministic.
7. Verify HUD text and indicators, full braking distance, no Console exceptions,
   no excessive wheelspin or tunneling, and no loss of the vehicle off the surface.
8. Disable Movement while accelerating: no stale wheel torque should remain.
9. Build the enabled scene and repeat smoke testing in the desktop player.

These steps are pending, not claimed passes. Suspension tuning and an authored
circuit are required before calling the game playable or polished.

Exercises: derive the old pedal rise time at 144 FPS; calculate second-gear wheel
torque; compare stopping distance before/after the brake split fix; graph camera
error at sharpness 3 versus 6; propose an input-provider interface and a race
state diagram without allowing HUD code to mutate simulation state.

## Milestone 1 follow-up: wheel presentation

User Play Mode testing confirmed the car moves and steers, but the visible wheels
stayed angled. The demo had fixed front-wheel rotation overrides (approximately
15 and 24 degrees away from the prefab orientation). No script animated the meshes.
Removed those overrides and moved each mesh filter/renderer onto a WheelVisual
child, keeping the WheelCollider on its original stationary mounting transform.
Movement now calls GetWorldPose in LateUpdate for all four paired visuals. This
includes steering, rolling and suspension travel. Setting the collider transform
instead would feed animation back into physics and progressively move the mount.

The serialized array follows the existing collider order: front left, front right,
rear left, rear right. The earlier audit's right/left labels were reversed; axle
membership was correct, so drive and brake allocation were unaffected.

Reference: https://docs.unity3d.com/ja/current/ScriptReference/WheelCollider.GetWorldPose.html

Validation: static prefab ownership, unique IDs and visual/collider references
checked. Compilation and visual acceptance remain pending. Stop Play Mode before
reloading the modified prefab. On a fresh run, hold A then D while stationary:
front meshes should visibly turn both ways and center on release; rear meshes
should remain straight. Accelerate to check rolling and suspension alignment.
Use Scene view close-up if the chase view is too small. If the imported mesh axes
or pivots produce a flipped/off-center visual, inspect the visual transform before
adding a model-specific alignment offset; do not rotate the physics mount to hide it.

## User Play Mode validation — 2026-10-23

In Unity 6000.1.0f1, the user confirmed the F1 Demo car moves and steering works.
After the wheel presentation fix (1713d20), the user confirmed the wheel visuals
work as well. This validates those observed behaviors in the Editor; it does not
establish acceptance of frame-rate comparisons, braking distance, suspension
alignment under all conditions, or a desktop release build.

Next milestone: a closed circuit, ordered checkpoints and recovery to the last
valid checkpoint, followed by the countdown/lap/results/restart race loop.
