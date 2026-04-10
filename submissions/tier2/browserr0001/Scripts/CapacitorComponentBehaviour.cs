using System;
using UnityEngine;
using UnityEngine.Events;

namespace DahVarsityV2.Components
{
    /// <summary>
    /// Drives a physical capacitor prefab in the Smart Glasses workbench scene.
    ///
    /// Architecture rules followed:
    ///   - All tunable values come from GlassesComponentDataSO (no hardcoded numbers).
    ///   - No FindObjectOfType, no static classes.
    ///   - Dependencies are injected via Inspector references or UnityEvents.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CapacitorComponentBehaviour : MonoBehaviour
    {
        // ── Inspector wiring ──────────────────────────────────────────────────
        [Header("Data (ScriptableObject — do not hardcode values here)")]
        [SerializeField] private GlassesComponentDataSO _data;

        [Header("Workbench Slot Reference")]
        [Tooltip("Drag the WorkbenchSlot transform from the scene into this field.")]
        [SerializeField] private Transform _targetSlot;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Renderer _bandRenderer;   // stripe on capacitor body

        [Header("Events — wire to circuit manager via Inspector")]
        public UnityEvent<GlassesComponentDataSO> OnComponentPlaced;
        public UnityEvent<GlassesComponentDataSO> OnComponentPickedUp;
        public UnityEvent<GlassesComponentDataSO> OnCircuitCompleted;

        // ── Runtime state ─────────────────────────────────────────────────────
        private Rigidbody _rb;
        private bool _isHeld;
        private bool _isPlaced;
        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_data == null)
            {
                Debug.LogError($"[{nameof(CapacitorComponentBehaviour)}] No GlassesComponentDataSO assigned on {name}. Disabling.");
                enabled = false;
                return;
            }

            _rb = GetComponent<Rigidbody>();
            ApplyVisuals();
        }

        private void Start()
        {
            // Nothing injected via Start — all wiring is done in the Inspector.
        }

        // ── Public API (called by input/grab system) ───────────────────────────

        /// <summary>Called by the grab input handler when the player picks this up.</summary>
        public void OnGrab()
        {
            if (_isPlaced) return;

            _isHeld = true;
            _rb.isKinematic = true;
            OnComponentPickedUp?.Invoke(_data);
        }

        /// <summary>Called by the grab input handler when the player releases.</summary>
        public void OnRelease()
        {
            _isHeld = false;

            if (_targetSlot != null && IsNearSlot())
            {
                SnapToSlot();
            }
            else
            {
                _rb.isKinematic = false;   // let physics take over
            }
        }

        /// <summary>Call this every frame from the grab system to move the component with the cursor.</summary>
        public void MoveToPosition(Vector3 worldPosition)
        {
            if (!_isHeld) return;
            transform.position = worldPosition;
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private bool IsNearSlot()
        {
            float threshold = _data.snapDistance;
            return Vector3.Distance(transform.position, _targetSlot.position) <= threshold;
        }

        private void SnapToSlot()
        {
            _isPlaced = true;
            _rb.isKinematic = true;

            transform.position = _targetSlot.position;
            transform.rotation = _targetSlot.rotation;

            SetHighlight(true);
            OnComponentPlaced?.Invoke(_data);

            if (_data.completesCircuit)
                OnCircuitCompleted?.Invoke(_data);

            Debug.Log($"[Capacitor] {_data.componentName} snapped to slot {_data.targetSlotIndex}. " +
                      $"C={_data.capacitance * 1e6f:F0} µF, ESR={_data.esr * 1000f:F0} mΩ, " +
                      $"Vrated={_data.ratedVoltage} V");
        }

        private void ApplyVisuals()
        {
            if (_bodyRenderer != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor(ColorProp, _data.componentColor);
                _bodyRenderer.SetPropertyBlock(mpb);
            }

            if (_bandRenderer != null)
            {
                // Capacitance band: lighter tint of the body colour
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor(ColorProp, _data.componentColor * 1.4f);
                _bandRenderer.SetPropertyBlock(mpb);
            }
        }

        private void SetHighlight(bool on)
        {
            if (_bodyRenderer == null) return;
            var mpb = new MaterialPropertyBlock();
            mpb.SetColor(ColorProp, on ? Color.white : _data.componentColor);
            _bodyRenderer.SetPropertyBlock(mpb);
        }

        // ── Accessors ─────────────────────────────────────────────────────────

        /// <summary>Read-only access to the data asset (for circuit manager queries).</summary>
        public GlassesComponentDataSO Data => _data;
        public bool IsPlaced => _isPlaced;
    }
}
