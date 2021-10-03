# Changelog

## [1.7.1] - Unreleased

### Changed

- Force module Rewrited
- Calculate force into speed instead of position
- Average speed with last frame

## [1.6.1] - 2020-04-06

### Fixed

- EZNestedEditorAttribute is not suitable for EZSoftBoneMaterial (Default-Material editing should be disabled)

### Changed

- Node can be moved freely if its depth is less than startDepth
- Modified property orders in Inspector

### Added

- Custom startDepth can be specified with function RevertTransforms(int startDepth)

## [1.6.0] - 2020-03-26

### Changed

- EZSoftBoneForce: it drives from ScriptObject now
- EZSoftBone: a force space could be specified for EZSoftBoneForce

## [1.5.2] - 2020-01-06

### Changed

- Garbage Collection Optimize: replace Mathf.Max(a, b, c) with Mathf.Max(a, Mathf.Max(b, c))

## [1.5.1] - 2019-12-13

### Fixed

- Bug fixed on LengthUnification (Wrong length calculation)

### Added

- Added an custom inspector for `EZSoftBone`, not all changes will trigger a reconstruction now

### Changed

- Changed some function names

## [1.5.0] - 2019-12-11

### Added

- End Bones: end bones can be specified
- Length Unification: there are 3 length calculation modes now, just like the "Sibling Constraints"
- Add set accessor to some properties
- Add a public function `Reconstructure` so you can reinitialize the system after you changed properties at runtime.

### Changed

- Change some variables' name
- replace enum SiblingConstraintMode with UnificationMode

## [1.4.0] - 2019-11-25

### Changed

- Change Name to EZSoftBone.

## [1.3.0] - 2019-11-08

### Added

- GravityAligner: A transform can be specified to determine how much the gravity effects the system (inversely correlated to dot product of aligner's y direction and world's y direction)
- SimulateSpace: A transform can be specified as a simulate space, it's useful when the system needs to be updated with a moving object (like the hair in a car)

### Fixed

- Fixed wrong calculations on Iterations

### Changed

- Call RevertTransforms on Update instead of LateUpdate, InternalAnimationUpdate will be called between them

## [1.2.1] - 2019-10-11

### Added

- SiblingRotationConstraints: Rotation will be affected by Sibling Constraints if enabled
- Delta_Min: a constant value (1e-6), Pasue if deltaTime is under this value

## [1.2.0] - 2019-08-23

### Changed

- Now the restrictions' length will be scaled with the related transforms

### Removed

- End Node: That's modeler's responsibility (and remove it makes the code looks so much better)

## [1.1.0] - 2019-07-24

### Added

- Added nested editor for PBMaterials

### Changed

- Package path changed to Assets/EZhex1991/EZPhysicsBone
- Shorten material, collider, and force module's name
- Revised some comments

## [1.0.0] - 2019-06-10

- First Release