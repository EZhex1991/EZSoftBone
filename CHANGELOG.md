# Changelog

## [1.7.1] - 2026-10-19

### Changed

- Force module Rewritten
- Calculate force into speed instead of position
- Average speed with the last frame
- Removed unused parameter `iteration`
- **Performance** parameter section marked as Incomplete

## [1.6.1] - 2020-04-06

### Fixed

- EZNestedEditorAttribute was incompatible with EZSoftBoneMaterial (Default-Material editing should be disabled)

### Changed

- Nodes can be moved freely if their depth is less than `startDepth`
- Modified property order in the Inspector

### Added

- Custom `startDepth` can be specified via the function `RevertTransforms(int startDepth)`

## [1.6.0] - 2020-03-26

### Changed

- EZSoftBoneForce: now derives from ScriptableObject
- EZSoftBone: support specifying a force space for EZSoftBoneForce

## [1.5.2] - 2020-01-06

### Changed

- Garbage Collection Optimization: replace `Mathf.Max(a, b, c)` with `Mathf.Max(a, Mathf.Max(b, c))`

## [1.5.1] - 2019-12-13

### Fixed

- Fixed bug in `LengthUnification` (incorrect length calculation)

### Added

- Added a custom inspector for `EZSoftBone`; not all changes will trigger reconstruction now

### Changed

- Renamed some function

## [1.5.0] - 2019-12-11

### Added

- End Bones: end bones can now be specified
- Length Unification: 3 length-calculation modes are available, similar to "Sibling Constraints"
- Added set accessor for some properties
- Added public function `Reconstructure` to re-initialize the system after modifying properties at runtime.

### Changed

- Renamed some variables
- Replaced enum `SiblingConstraintMode` with `UnificationMode`

## [1.4.0] - 2019-11-25

### Changed

- Renamed to EZSoftBone.

## [1.3.0] - 2019-11-08

### Added

- GravityAligner: a transform can be assigned to control gravity influence on the system (inversely correlated to the dot product of the aligner’s Y‑axis and world Y‑axis)
- SimulateSpace: assign a transform as the simulation space; useful when the system needs to update alongside a moving object (e.g. hair inside a car)

### Fixed

- Fixed incorrect calculations for Iterations

### Changed

- Call `RevertTransforms` in `Update` instead of `LateUpdate`, `InternalAnimationUpdate` will be invoked between them

## [1.2.1] - 2019-10-11

### Added

- SiblingRotationConstraints: rotations are affected by Sibling Constraints if enabled
- Delta_Min: constant value (`1e-6`), simulation pasue if `deltaTime` falls below this threshold

## [1.2.0] - 2019-08-23

### Changed

- Restriction lengths are now scaled together with their associated transforms

### Removed

- End Node: this responsibility is left to modelers (removing it greatly improves code readability)

## [1.1.0] - 2019-07-24

### Added

- Added nested editor for PBMaterials

### Changed

- Package path changed to `Assets/EZhex1991/EZPhysicsBone`
- Shortened names for material, collider, and force module
- Revised some comments

## [1.0.0] - 2019-06-10

- First Release