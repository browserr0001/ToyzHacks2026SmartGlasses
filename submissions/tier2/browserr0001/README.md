# CapacitorModule — ToyzHacks 2026 Tier 2 Submission

**Component:** Electrolytic Capacitor (100 µF, 5 V rated)  
**Unity Version:** 6.3.7f1  
**Author:** browserr0001

---

## What This Is

A grabbable, snap-to-slot capacitor prefab for the Smart Glasses workbench scene. The player picks it up, carries it to the capacitor slot on the workbench, and it snaps in — completing the capacitor branch of the RC power circuit.

This ties directly to the RC circuit theme of Tier 1: the same physics (τ = RC) now lives in 3D as a physical component a player can *repair with their hands*.

---

## File Structure

```
Tier2/browserr0001/
  CapacitorModule.prefab          ← drop in Assets/DahVarsityV2/UI/Prefabs/ (sibling, not nested)
  CapacitorModuleDataSO.asset     ← drop in Assets/DahVarsityV2/Data/ (or alongside prefab)
  Scripts/
    GlassesComponentDataSO.cs     ← ScriptableObject data definition
    CapacitorComponentBehaviour.cs ← MonoBehaviour driving the prefab
  README.md                       ← this file
```

---

## Architecture Rules Compliance

| Rule | How this submission follows it |
|---|---|
| ScriptableObject pattern | All values live in `GlassesComponentDataSO` — nothing hardcoded in `CapacitorComponentBehaviour` |
| No `FindObjectOfType` | Zero uses. Dependencies injected via Inspector fields and UnityEvents |
| No static classes | None used |
| Prefab as sibling in `Assets/DahVarsityV2/UI/Prefabs/` | Prefab is a root-level sibling — not nested inside another prefab |
| Edit in Prefab Edit Mode only | Documented in setup steps below |
| Reference `GlassesComponentDataSO` | `_data` field is the only source of truth for all tunable values |

---

## How to Set Up in the Scene

1. **Import scripts** — place both `.cs` files in `Assets/DahVarsityV2/Scripts/Components/`.
2. **Create the Data Asset** — right-click in the Project window → *DahVarsity → Component Data* → rename to `CapacitorModuleDataSO` and set values (or use the provided `.asset`).
3. **Place the Prefab** — drag `CapacitorModule.prefab` into `Assets/DahVarsityV2/UI/Prefabs/` as a sibling.
4. **Open in Prefab Edit Mode** — double-click the prefab in the Project window. In the Inspector, assign:
   - `_data` → `CapacitorModuleDataSO`
   - `_bodyRenderer` → `CapacitorBody` child's MeshRenderer
   - `_bandRenderer` → `CapacitorBand` child's MeshRenderer
5. **Wire the slot** — in the scene, assign the `WorkbenchSlot` transform to `_targetSlot` on the prefab instance's overrides (or via the scene injection pattern your architecture uses).
6. **Wire events** — connect `OnComponentPlaced` and `OnCircuitCompleted` to your circuit manager via the Inspector. The events pass the `GlassesComponentDataSO` so the manager can read `capacitance`, `targetSlotIndex`, etc. without coupling.

---

## Grab System Integration

`CapacitorComponentBehaviour` exposes three public methods for your grab/input system to call:

```csharp
capacitor.OnGrab();                      // player picks it up
capacitor.MoveToPosition(worldPos);      // call every frame while held
capacitor.OnRelease();                   // player lets go — auto-snaps if close enough
```

No polling, no static references — call these from whatever input handler your project uses (XR Interaction Toolkit, custom raycast, WASD+E, etc.).

---

## Electrical Values (from the DataSO)

| Property | Value | Notes |
|---|---|---|
| Capacitance | 100 µF | τ = RC, same as Tier 1 simulation |
| ESR | 50 mΩ | Low-ESR for power rail use |
| Rated Voltage | 5 V | Smart Glasses PMIC output rail |
| Snap Distance | 0.15 m | Generous snap zone for usability |
| Completes Circuit | ✅ | Fires `OnCircuitCompleted` on placement |

---

## Extending This Component

To add a different capacitor (e.g. 10 µF ceramic for bypass):

1. *Right-click → DahVarsity → Component Data* → create a new asset.
2. Set `capacitance = 0.00001`, `targetSlotIndex = 1`, `completesCircuit = false`, pick a different color.
3. Duplicate the prefab, assign the new data asset. Done — no code changes needed.
