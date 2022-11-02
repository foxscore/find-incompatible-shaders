using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fox_score.Find_Incompatible_Shaders
{
    [Serializable]
    public class MaterialEntry
    {
        public bool foldout;
        
        [SerializeField] private List<MaterialUsage> materialUsages;
        [SerializeField] private Texture previewTexture;
        [SerializeField] private Material materialCopy;
        [SerializeField] private Material material;
        [SerializeField] private Shader originalShader;
        
        /// <summary>
        /// DO NOT CHANGE ANYTHING ON THIS MATERIAL
        /// </summary>
        public Material Target => material;
        public Shader OriginalShader => originalShader;
        
        public Texture PreviewTexture
        {
            get
            {
                if (previewTexture == null)
                    previewTexture = AssetPreview.GetAssetPreview(materialCopy);
                return previewTexture;
            }
        }
        
        public Shader ReplacementShader
        {
            get => materialCopy.shader;
            set => GenerateMaterialCopy(value);
        }

        public MaterialEntry(Material material, MaterialUsage initialUsage)
        {
            materialUsages = new List<MaterialUsage> { initialUsage };
            this.material = material;
            originalShader = material.shader;
            
            GenerateMaterialCopy(originalShader);
        }
        
        public void Reset() => GenerateMaterialCopy(originalShader);

        public void Apply()
        {
            material.shader = ReplacementShader;
            EditorUtility.SetDirty(material);
        }
        
        // originalShader == null || ReplacementShader != originalShader
        public bool ShaderChanged => ReplacementShader != material.shader;

        private void GenerateMaterialCopy(Shader shader)
        {
            materialCopy = new Material(material)
            {
                name = material.name + " (preview)",
                shader = shader
            };
            previewTexture = null;
        }
        
        public void AddUsage(MaterialUsage usage) => materialUsages.Add(usage);
        public void AddUsages(IEnumerable<MaterialUsage> usages) => materialUsages.AddRange(usages);
        public void AddUsages(MaterialEntry other) => AddUsages(other.materialUsages);
        public void RemoveUsage(MaterialUsage usage) => materialUsages.Remove(usage);
        public MaterialUsage[] GetUsages() => materialUsages.ToArray();

        public bool HasConflictingUsages()
        {
            return
#if !UNITY_ANDROID || REVS_READY
                false;
#else
                materialUsages.Any(usage => usage.type < MaterialUsageType.ParticleSystemAnimationClip) &&
                materialUsages.Any(usage => usage.type == MaterialUsageType.ParticleSystemAnimationClip);
#endif
        }
        public bool HasOnlyParticleSystemAnimationClipUsages()
            => materialUsages.All(usage => usage.type == MaterialUsageType.ParticleSystemAnimationClip);
    }
}