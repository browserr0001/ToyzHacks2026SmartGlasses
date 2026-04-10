using UnityEngine;

namespace DahVarsityV2.Components
{
    /// <summary>
    /// Base ScriptableObject that stores all data for a Smart Glasses workbench component.
    /// Extend this or use it directly — never hardcode values in MonoBehaviours.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewGlassesComponentData",
        menuName = "DahVarsity/Component Data")]
    public class GlassesComponentDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string componentName = "Unnamed Component";
        [TextArea(2, 4)]
        public string description = "";

        [Header("Electrical Properties")]
        [Tooltip("Capacitance in Farads (e.g. 100e-6 for 100 µF)")]
        public float capacitance = 100e-6f;

        [Tooltip("Equivalent Series Resistance in Ohms")]
        public float esr = 0.05f;

        [Tooltip("Rated voltage limit in Volts")]
        public float ratedVoltage = 5f;

        [Header("Workbench Slot")]
        [Tooltip("Which slot index in the workbench this component fits")]
        public int targetSlotIndex = 0;

        [Tooltip("Does placing this component complete a circuit branch?")]
        public bool completesCircuit = false;

        [Header("Visuals")]
        public Color componentColor = new Color(0.2f, 0.6f, 1f);
        public float snapDistance = 0.15f;
    }
}
