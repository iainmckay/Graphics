using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace UnityEngine.Rendering.HighDefinition
{
    // This class is used to associate a unique ID to a sky class.
    // This is needed to be able to automatically register sky classes and avoid collisions and refactoring class names causing data compatibility issues.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SkyUniqueID : Attribute
    {
        public readonly int uniqueID;

        public SkyUniqueID(int uniqueID)
        {
            this.uniqueID = uniqueID;
        }
    }

    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public sealed class EnvUpdateParameter : VolumeParameter<EnvironmentUpdateMode>
    {
        public EnvUpdateParameter(EnvironmentUpdateMode value, bool overrideState = false)
            : base(value, overrideState) {}
    }

    public enum SkyIntensityMode
    {
        Exposure,
        Lux,
    }

    [System.Flags]
    public enum SkySettingsPropertyFlags
    {
        ShowMultiplierAndEV = (1 << 0),
        ShowRotation =        (1 << 1),
        ShowUpdateMode =      (1 << 2),
    }

    public enum BackplateType
    {
        Disc,
        Rectangle,
        Ellipse,
        Infinite
    }

    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public sealed class BackplateTypeParameter : VolumeParameter<BackplateType>
    {
        public BackplateTypeParameter(BackplateType value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public sealed class SkyIntensityParameter : VolumeParameter<SkyIntensityMode>
    {
        public SkyIntensityParameter(SkyIntensityMode value, bool overrideState = false)
            : base(value, overrideState) {}
    }

    public abstract class SkySettings : VolumeComponent
    {
        [Tooltip("Sets the rotation of the sky.")]
        public ClampedFloatParameter    rotation = new ClampedFloatParameter(0.0f, 0.0f, 360.0f);
        [Tooltip("Specifies the intensity mode HDRP uses for the sky.")]
        public SkyIntensityParameter    skyIntensityMode = new SkyIntensityParameter(SkyIntensityMode.Exposure);
        [Tooltip("Sets the exposure of the sky in EV.")]
        public FloatParameter           exposure = new FloatParameter(0.0f);
        [Tooltip("Sets the intensity multiplier for the sky.")]
        public MinFloatParameter        multiplier = new MinFloatParameter(1.0f, 0.0f);
        [Tooltip("Informative helper that displays the relative intensity (in Lux) for the current HDR texture set in HDRI Sky.")]
        public MinFloatParameter        upperHemisphereLuxValue = new MinFloatParameter(1.0f, 0.0f);
        [Tooltip("Informative helper that displays Show the color of Shadow.")]
        public Vector3Parameter         upperHemisphereLuxColor = new Vector3Parameter(new Vector3(0, 0, 0));
        [Tooltip("Sets the absolute intensity (in Lux) of the current HDR texture set in HDRI Sky. Functions as a Lux intensity multiplier for the sky.")]
        public FloatParameter           desiredLuxValue = new FloatParameter(20000);
        [Tooltip("Specifies when HDRP updates the environment lighting. When set to OnDemand, use HDRenderPipeline.RequestSkyEnvironmentUpdate() to request an update.")]
        public EnvUpdateParameter       updateMode = new EnvUpdateParameter(EnvironmentUpdateMode.OnChanged);
        [Tooltip("Sets the period, in seconds, at which HDRP updates the environment ligting (0 means HDRP updates it every frame).")]
        public MinFloatParameter        updatePeriod = new MinFloatParameter(0.0f, 0.0f);
        [Tooltip("When enabled, HDRP uses the Sun Disk in baked lighting.")]
        public BoolParameter            includeSunInBaking = new BoolParameter(false);

        // Unused for now. In the future we might want to expose this option for very high range skies.
        bool m_useMIS = false;
        public bool useMIS { get { return m_useMIS; } }

        static Dictionary<Type, int>  skyUniqueIDs = new Dictionary<Type, int>();

        public override int GetHashCode()
        {
            unchecked
            {
                // UpdateMode and period should not be part of the hash as they do not influence rendering itself.
                int hash = 13;
                hash = hash * 23 + rotation.GetHashCode();
                hash = hash * 23 + exposure.GetHashCode();
                hash = hash * 23 + multiplier.GetHashCode();
                hash = hash * 23 + desiredLuxValue.GetHashCode();
                hash = hash * 23 + skyIntensityMode.GetHashCode();
                hash = hash * 23 + includeSunInBaking.GetHashCode();

                return hash;
            }
        }

        public static int GetUniqueID<T>()
        {
            return GetUniqueID(typeof(T));
        }

        public static int GetUniqueID(Type type)
        {
            int uniqueID;

            if (!skyUniqueIDs.TryGetValue(type, out uniqueID))
            {
                var uniqueIDs = type.GetCustomAttributes(typeof(SkyUniqueID), false);
                uniqueID = (uniqueIDs.Length == 0) ? -1 : ((SkyUniqueID)uniqueIDs[0]).uniqueID;
                skyUniqueIDs[type] = uniqueID;
            }

            return uniqueID;
        }

        public abstract Type GetSkyRendererType();
    }
}
