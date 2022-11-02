using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Fox_score.Find_Incompatible_Shaders
{
    public static class Lookout
    {
        public const string ShaderNameMissing = "Hidden/InternalErrorShader";
        public const string ShaderNameBase = "VRChat/Mobile/";

        // It must be in the shader collection
        private static bool IsLegalShaderName(string shader) => ShaderCollectionLoader.VrcShaders.shaders.Any(s => s.name == shader);
        private static bool IsLegalParticleShaderName(string shader) => ShaderCollectionLoader.VrcParticleShaders.shaders.Any(s => s.name == shader);
        
        public static MaterialEntryCollection FindIncompatibleMaterials(VRCAvatarDescriptor avatar)
        {
            const string progressBarTitle = "Looking for incompatible materials";
            
            var incompatibleMaterials = new MaterialEntryCollection();

            // Get all materials on the avatar that don't use a VRChat/Mobile shader, and that aren't missing a shader

            void ScanController(GameObject root, RuntimeAnimatorController controller, string name = null)
            {
                if (controller == null) return;

                // Get all animations on the animator
                var animations = controller.animationClips;
                for (var i = 0; i < animations.Length; i++)
                {
                    var animationClip = animations[i];
                    if (name != null)
                        EditorUtility.DisplayProgressBar(
                            progressBarTitle,
                        $"Looking for materials on animation {animationClip.name}",
                            (float)i / animations.Length
                        );

                    // Get all materials on the animation that don't use a VRChat/Mobile shader and that aren't missing a shader
                    // In cases where the animation track effects a particle system, check if its a legal particle shader
                    // var editorCurveBindings = AnimationUtility.GetCurveBindings(animationClip);
                    var editorCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                    for (var j = 0; j < editorCurveBindings.Length; j++)
                    {
                        var editorCurveBinding = editorCurveBindings[j];
                        if (
                            !editorCurveBinding.propertyName.StartsWith("m_Material") &&
                            !editorCurveBinding.propertyName.StartsWith("Material")
                        ) continue;
                        
                        var objectReferenceKeyframes =
                            AnimationUtility.GetObjectReferenceCurve(animationClip, editorCurveBinding);
                        
                        // Check if we are targeting a particle system renderer
                        var particleSystemRenderer = AnimationUtility.GetAnimatedObject(root, editorCurveBinding) as ParticleSystemRenderer;
                        if (particleSystemRenderer != null)
                        {
                            // For each material on this curve, check if its a legal particle shader
                            foreach (var keyframe in objectReferenceKeyframes)
                            {
                                var material = keyframe.value as Material;
                                if (material == null) continue;
                                if (material.shader == null) continue;
                                if (material.shader.name == ShaderNameMissing) continue;
                                var usage = new MaterialUsage(animationClip, MaterialUsageType.ParticleSystemAnimationClip);
                                if (IsLegalParticleShaderName(material.shader.name))
                                    incompatibleMaterials.AddLegalUsage(material, usage);
                                else
                                    incompatibleMaterials.Add(new MaterialEntry(material, usage));
                            }
                            continue;
                        }

                        // Check if it's a legal shader
                        foreach (var keyframe in objectReferenceKeyframes)
                        {
                            var material = keyframe.value as Material;
                            if (material == null) continue;
                            if (material.shader == null) continue;
                            if (material.shader.name == ShaderNameMissing) continue;
                            var usage = new MaterialUsage(animationClip, MaterialUsageType.AnimationClip);
                            if (IsLegalShaderName(material.shader.name))
                                incompatibleMaterials.AddLegalUsage(material, usage);
                            else
                                incompatibleMaterials.Add(new MaterialEntry(material, usage));
                        }
                    }
                }
            }

            var totalChildren = 1;
            void ScanGameObjectRecursive(GameObject gameObject, string path)
            {
                totalChildren += gameObject.transform.childCount;
                
                EditorUtility.DisplayProgressBar(
                    progressBarTitle,
                    $"Looking for materials on {path}",
                    (float)totalChildren / avatar.transform.childCount
                );
                
                // Get all Renderers on the GameObject
                foreach (var renderer in gameObject.GetComponents<Renderer>())
                {
                    // MeshRenderer, SkinnedMeshRenderer, ParticleSystemRenderer, or Other
                    var type = renderer is SkinnedMeshRenderer
                        ? MaterialUsageType.SkinnedMeshRenderer
                        : renderer is MeshRenderer
                            ? MaterialUsageType.MeshRenderer
                            : renderer is ParticleSystemRenderer
                                ? MaterialUsageType.ParticleSystem
                                : MaterialUsageType.Other;
                    var materials = renderer.sharedMaterials.Where(m => m != null);
                    foreach (var material in materials)
                    {
                        var usage = new MaterialUsage(gameObject, type);
                        if (IsLegalShaderName(material.shader.name))
                            incompatibleMaterials.AddLegalUsage(material, usage);
                        else
                            incompatibleMaterials.Add(new MaterialEntry(material, usage));
                    }
                }

                // Get all animators on the GameObject
                foreach (var animator in gameObject.GetComponents<Animator>())
                    ScanController(gameObject, animator.runtimeAnimatorController);

                // Scan all children
                for (var i = 0; i < gameObject.transform.childCount; i++)
                    ScanGameObjectRecursive(gameObject.transform.GetChild(i).gameObject, path + "/" + gameObject.name);
            }

            var go = avatar.gameObject;
            ScanGameObjectRecursive(go, go.name);

            if (avatar.customizeAnimationLayers)
            {
                foreach (var layer in avatar.baseAnimationLayers.Where(cal => !cal.isDefault))
                    ScanController(avatar.gameObject, layer.animatorController, $"Avatar ({layer.type.ToString()})");

                foreach (var layer in avatar.specialAnimationLayers.Where(cal => !cal.isDefault))
                    ScanController(avatar.gameObject, layer.animatorController, $"Avatar ({layer.type.ToString()})");
            }

            // Flush legal usages
            EditorUtility.DisplayProgressBar(progressBarTitle, "Finalizing", 1);
            incompatibleMaterials.FlushLegalUsages();

            EditorUtility.ClearProgressBar();
            
            return incompatibleMaterials;
        }
    }
}