using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    [CanEditMultipleObjects]
    [VolumeComponentEditor(typeof(CloudLayer))]
    class CloudLayerEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_Enabled;

        SerializedDataParameter m_CloudMap;
        SerializedDataParameter m_UpperHemisphereOnly;
        SerializedDataParameter m_Tint;
        SerializedDataParameter m_IntensityMultiplier;
        SerializedDataParameter m_Rotation;

        SerializedDataParameter m_EnableDistortion;
        SerializedDataParameter m_Procedural;
        SerializedDataParameter m_Flowmap;
        SerializedDataParameter m_ScrollDirection;
        SerializedDataParameter m_ScrollSpeed;

        SerializedDataParameter m_CloudShadows;
        SerializedDataParameter m_ShadowOpacity;
        SerializedDataParameter m_ShadowScale;

        SerializedDataParameter m_CloudLighting;
        SerializedDataParameter m_SunLightColor;
        SerializedDataParameter x1, x2, x3;

        GUIContent[]    m_DistortionModes = { new GUIContent("Procedural"), new GUIContent("Flowmap") };
        int[]           m_DistortionModeValues = { 1, 0 };

        MaterialEditor  materialEditor = null; 

        public override void OnEnable()
        {
            var o = new PropertyFetcher<CloudLayer>(serializedObject);

            m_Enabled                   = Unpack(o.Find(x => x.enabled));

            m_CloudMap                  = Unpack(o.Find(x => x.cloudMap));
            m_UpperHemisphereOnly       = Unpack(o.Find(x => x.upperHemisphereOnly));
            m_Tint                      = Unpack(o.Find(x => x.tint));
            m_IntensityMultiplier       = Unpack(o.Find(x => x.intensityMultiplier));
            m_Rotation                  = Unpack(o.Find(x => x.rotation));

            m_EnableDistortion          = Unpack(o.Find(x => x.enableDistortion));
            m_Procedural                = Unpack(o.Find(x => x.procedural));
            m_Flowmap                   = Unpack(o.Find(x => x.flowmap));
            m_ScrollDirection           = Unpack(o.Find(x => x.scrollDirection));
            m_ScrollSpeed               = Unpack(o.Find(x => x.scrollSpeed));

            m_CloudShadows              = Unpack(o.Find(x => x.cloudShadows));
            m_ShadowOpacity             = Unpack(o.Find(x => x.shadowOpacity));
            m_ShadowScale               = Unpack(o.Find(x => x.shadowScale));


            m_CloudLighting             = Unpack(o.Find(x => x.cloudLighting));
            m_SunLightColor             = Unpack(o.Find(x => x.sunLightColor));
            x1               = Unpack(o.Find(x => x.x1));
            x2               = Unpack(o.Find(x => x.x2));
            x3               = Unpack(o.Find(x => x.x3));

            CreateEditor(m_CloudMap);
        }

        public override void OnDisable ()
        {
            if (materialEditor != null)
                Object.DestroyImmediate(materialEditor);

            base.OnDisable();
        }

        bool IsMapFormatInvalid(SerializedDataParameter map)
        {
            if (!map.overrideState.boolValue || map.value.objectReferenceValue == null)
                return false;
            var tex = map.value.objectReferenceValue;
            if (!tex.GetType().IsSubclassOf(typeof(Texture)))
                return true;
            return (tex as Texture).dimension != TextureDimension.Tex2D;
        }

        void CreateEditor(SerializedDataParameter map)
        {
            if (materialEditor != null)
                Object.DestroyImmediate(materialEditor);

            var tex = map.value.objectReferenceValue as CustomRenderTexture;
            if (tex != null && tex.material != null)
                materialEditor = (MaterialEditor)Editor.CreateEditor(tex.material);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(m_Enabled, new GUIContent("Enable"));

            EditorGUI.BeginChangeCheck ();
            PropertyField(m_CloudMap);
            if (EditorGUI.EndChangeCheck())
                CreateEditor(m_CloudMap);

            if (IsMapFormatInvalid(m_CloudMap))
                EditorGUILayout.HelpBox("The cloud map needs to be a 2D Texture in LatLong layout.", MessageType.Info);

            PropertyField(m_UpperHemisphereOnly);
            PropertyField(m_Tint);
            PropertyField(m_IntensityMultiplier);
            PropertyField(m_Rotation);

            PropertyField(m_EnableDistortion);
            if (m_EnableDistortion.value.boolValue)
            {
                EditorGUI.indentLevel++;

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawOverrideCheckbox(m_Procedural);
                    using (new EditorGUI.DisabledScope(!m_Procedural.overrideState.boolValue))
                        m_Procedural.value.boolValue = EditorGUILayout.IntPopup(new GUIContent("Distortion Mode"), (int)m_Procedural.value.intValue, m_DistortionModes, m_DistortionModeValues) == 1;
                }

                if (!m_Procedural.value.boolValue)
                {
                    EditorGUI.indentLevel++;
                    PropertyField(m_Flowmap);
                    if (IsMapFormatInvalid(m_Flowmap))
                        EditorGUILayout.HelpBox("The flowmap needs to be a 2D Texture in LatLong layout.", MessageType.Info);
                    EditorGUI.indentLevel--;
                }

                PropertyField(m_ScrollDirection);
                PropertyField(m_ScrollSpeed);
                EditorGUI.indentLevel--;
            }

            PropertyField(m_CloudShadows, new GUIContent("Enable Cloud Shadows"));
            if (m_CloudShadows.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(m_ShadowOpacity, new GUIContent("Opacity"));
                PropertyField(m_ShadowScale, new GUIContent("Scale"));
                EditorGUI.indentLevel--;
            }

            PropertyField(m_CloudLighting, new GUIContent("Enable Cloud Lighting"));
            if (m_CloudLighting.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(m_SunLightColor);
                PropertyField(x2, new GUIContent("Sun Intensity"));
                PropertyField(x1, new GUIContent("Raymarch distance"));
                PropertyField(x3, new GUIContent("A multiplier"));
                EditorGUI.indentLevel--;
            }

            if (materialEditor != null)
            {
                EditorGUILayout.Space();
                materialEditor.DrawHeader(); 
                materialEditor.OnInspectorGUI(); 
                EditorGUILayout.Space();
            }
        }
    }
}
