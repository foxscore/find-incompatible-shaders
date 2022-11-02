using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Fox_score.Find_Incompatible_Shaders
{
    public sealed class FindIncompatibleShaders : EditorWindow
    {
        private VRCAvatarDescriptor[] _avatarDescriptors = Array.Empty<VRCAvatarDescriptor>();
        private int _avatarIndex;
        private MaterialEntryCollection _incompatibleMaterials;
        private Vector2 _scrollPosition = Vector2.zero;
        private string[] _vrcShaderNames;
        private string[] _vrcParticleShaderNames;

        [MenuItem("Tools/Fox_score/Find Incompatible Shaders")]
        public static void ShowWindow()
        {
            var window = GetWindow<FindIncompatibleShaders>();
            window.titleContent = new GUIContent("Find Incompatible Shaders");
            window.minSize = new Vector2(500, 208);
            window.Show();
        }

        private void OnEnable()
        {
            _vrcShaderNames = Resources.Load<ShaderCollection>("Foxy/VrchatMobileShaders").shaders
                .Select(x => x.name.Substring(Lookout.ShaderNameBase.Length))
                .ToArray();
            
            _vrcParticleShaderNames = Resources.Load<ShaderCollection>("Foxy/VrchatMobileParticleShaders").shaders
                .Select(x => x.name.Substring(Lookout.ShaderNameBase.Length + ("Particles/").Length))
                .ToArray();
            
            FindAvatars();
            EditorApplication.hierarchyChanged += Update;
        }

        private void OnDisable() => EditorApplication.hierarchyChanged -= Update;

        private void Update()
        {
            FindAvatars();
            Repaint();
        }

        private void OnGUI()
        {
            DrawAvatarSection();
            EditorGUILayout.Separator();
            DrawIncompatibleMaterials();
        }

        void FindAvatars()
        {
            _avatarDescriptors = FindObjectsOfType<VRCAvatarDescriptor>()
                .Where(vad => vad.enabled && vad.gameObject.activeInHierarchy)
                .ToArray();
        }

        void DrawAvatarSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            _avatarIndex = EditorGUILayout.Popup("Avatar", _avatarIndex,
                _avatarDescriptors.Select(x => x.gameObject.name).ToArray());
            if (GUILayout.Button("Refresh", GUILayout.Width(100))) FindAvatars();
            EditorGUILayout.EndHorizontal();

            // Find incompatible materials button
            using (new EditorGUI.DisabledScope(_avatarDescriptors.Length == 0))
                // if (GUILayout.Button("Find Incompatible Materials"))
                if (GUILayout.Button("Find Incompatible Shader"))
                    _incompatibleMaterials = Lookout.FindIncompatibleMaterials(_avatarDescriptors[_avatarIndex]);

            EditorGUILayout.EndVertical();
        }

        private void DrawIncompatibleMaterials()
        {
            if (_incompatibleMaterials == null)
            {
                EditorGUILayout.HelpBox(
                    "Click \"Find Incompatible Shaders\" to find materials that don't use a VRChat/Mobile shader.",
                    MessageType.Info);
                return;
            }

            if (_incompatibleMaterials.Count == 0)
            {
                EditorGUILayout.HelpBox("No incompatible shaders found.", MessageType.Info);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (var i = 0; i < _incompatibleMaterials.Count; i++)
            {
                var entry = _incompatibleMaterials[i];

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // Draw material preview
                const int previewSize = 64;
                const int previewPadding = 2;
                var rect = GUILayoutUtility.GetRect(previewSize + previewPadding, previewSize,
                    GUILayout.ExpandWidth(false));
                rect.width = previewSize;
                if (entry.PreviewTexture != null)
                    GUI.DrawTexture(rect, entry.PreviewTexture);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                // Draw material name
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField(entry.Target, typeof(Material), false);
                // Draw shader name
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField(entry.OriginalShader, typeof(Shader), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                // Dropdown to select a replacement shader
                var hasOnlyParticleAnimation = entry.HasOnlyParticleSystemAnimationClipUsages();
                var collection = hasOnlyParticleAnimation
                    ? ShaderCollectionLoader.VrcParticleShaders
                    : ShaderCollectionLoader.VrcShaders;
                var index = -1;
                if (entry.ReplacementShader.name.StartsWith(Lookout.ShaderNameBase))
                {
                    for (var j = 0; j < collection.shaders.Length; j++)
                    {
                        var entryShader = collection.shaders[j];
                        if (entryShader == null) continue;
                        if (entryShader.name != entry.ReplacementShader.name) continue;
                        index = j;
                        break;
                    }
                }

                var shaderNames = hasOnlyParticleAnimation ? _vrcParticleShaderNames : _vrcShaderNames;
                var shaderIndex = EditorGUILayout.Popup(
                    new GUIContent("Replacement Shader", index == -1 ? "" : shaderNames[index]),
                    index,
                    shaderNames
                );
            
                if (shaderIndex != index && shaderIndex != -1)
                    entry.ReplacementShader = collection.shaders[shaderIndex];

                // Button to revert to original shader
                using (new EditorGUI.DisabledScope(!entry.ShaderChanged))
                {
                    if (GUILayout.Button("Reset", GUILayout.Width(50)))
                        entry.Reset();

                    if (GUILayout.Button("Apply", GUILayout.Width(50)))
                        entry.Apply();
                }

                EditorGUILayout.EndHorizontal();

                void DrawUsage(MaterialUsage usage, bool indent)
                {
                    Texture icon;
                    string tooltip;
                    switch (usage.type)
                    {
                        default:
                        case MaterialUsageType.Other:
                            icon = IconLoader.Other;
                            tooltip = $"Unknown ({usage.target.GetType().Name})";
                            break;
                        case MaterialUsageType.MeshRenderer:
                            icon = IconLoader.MeshRenderer;
                            tooltip = "Mesh Renderer";
                            break;
                        case MaterialUsageType.SkinnedMeshRenderer:
                            icon = IconLoader.SkinnedMeshRenderer;
                            tooltip = "Skinned Mesh Renderer";
                            break;
                        case MaterialUsageType.ParticleSystem:
                            icon = IconLoader.ParticleSystem;
                            tooltip = "Particle System";
                            break;
                        case MaterialUsageType.AnimationClip:
                            icon = IconLoader.AnimationClip;
                            tooltip = "Animation Clip";
                            break;
                        case MaterialUsageType.ParticleSystemAnimationClip:
                            icon = IconLoader.ParticleSystemAnimationClip;
                            tooltip = "Animation Clip targeting a Particle System";
                            break;
                    }

                    // Clickable label
                    rect = GUILayoutUtility.GetRect(0, 21, GUILayout.ExpandWidth(true));
                    if (indent)
                    {
                        // Move rect to the right by one indent level
                        rect.x += 15;
                        rect.width -= 15;
                    }
                    // Move up a little bit
                    rect.y -= 2;
                    // Draw GUIContent
                    if (GUI.Button(
                            rect,
                            new GUIContent(" " + usage.DisplayName, icon, tooltip),
                            EditorStyles.linkLabel
                        ))
                        Selection.activeObject = usage.target;
                }

                var usages = entry.GetUsages();
                if (usages.Length == 1)
                    DrawUsage(usages[0], false);
                else
                {
                    entry.foldout = EditorGUILayout.Foldout(entry.foldout, $" {usages.Length} usages", true);
                    if (entry.foldout)
                        foreach (var usage in usages)
                            DrawUsage(usage, true);
                }

                if (entry.HasConflictingUsages())
                {
                    // Material is used by particle system and non-particle system - potentially unwanted transparency issues
                    EditorGUILayout.HelpBox(
                        "This material is used by both a renderer and an animation targeting a particle system. " +
                        "If transparency is desired, consider not having an initial material for the particle system to get around this limitation.",
                        MessageType.Warning
                    );
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}