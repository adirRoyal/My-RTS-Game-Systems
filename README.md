

🎮 Camera Control:

Free movement (WASD / Arrow Keys).

Accelerated movement with boost key.

Smooth zoom in/out (with near and far limits).

Camera rotation using a button (usually RMB).

Built using the modern Input Actions system (InputManager).

🖱️ Input Based on Input System:

All commands are organized in RTSInputActions.

InputManager acts as a mediator and sends events to all systems.

Supports multiple platforms, extendable (for Mobile/Console in the future).

🟦 Unit Selection:

Single click to select/deselect.

Box selection (drag).

Shift support for multi-selection without losing current selection.

Square UI selector built and controlled via Canvas.

Tracks selected unit count (OnSelectionChanged event + UI).

🔁 Unit System:

SelectableUnit class supports selection/deselection + visuals.

Movement based on NavMeshAgent via UnitMovement.

Move via right-click command (ground only).

Visual ground marker (GroundMarkerController) displayed briefly.

🧠 Project Structure:

Uses events to reduce coupling.

Each class follows SRP (Single Responsibility Principle): input, camera, units, movement, UI.

Clear naming, modular system, easy to extend.

