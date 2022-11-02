using UnityEditor;
using UnityEngine;

namespace Fox_score.Find_Incompatible_Shaders
{
    public static class IconLoader
    {
        private static Texture _otherIcon;
        private static Texture _meshRendererIcon;
        private static Texture _skinnedMeshRendererIcon;
        private static Texture _particleSystemIcon;
        private static Texture _animationClipIcon;
        private static Texture _particleSystemAnimationClipIcon;

        public static Texture Other
        {
            get
            {
                if (_otherIcon == null)
                    _otherIcon = EditorGUIUtility.IconContent("_Help@2x").image;
                return _otherIcon;
            }
        }

        public static Texture MeshRenderer
        {
            get
            {
                if (_meshRendererIcon == null)
                    _meshRendererIcon = EditorGUIUtility.IconContent("MeshRenderer Icon").image;
                return _meshRendererIcon;
            }
        }

        public static Texture SkinnedMeshRenderer
        {
            get
            {
                if (_skinnedMeshRendererIcon == null)
                    _skinnedMeshRendererIcon = EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image;
                return _skinnedMeshRendererIcon;
            }
        }

        public static Texture ParticleSystem
        {
            get
            {
                if (_particleSystemIcon == null)
                    _particleSystemIcon = EditorGUIUtility.IconContent("ParticleSystem Icon").image;
                return _particleSystemIcon;
            }
        }

        public static Texture AnimationClip
        {
            get
            {
                if (_animationClipIcon == null)
                    _animationClipIcon = EditorGUIUtility.IconContent("AnimationClip Icon").image;
                return _animationClipIcon;
            }
        }

        public static Texture ParticleSystemAnimationClip
        {
            get
            {
                if (_particleSystemAnimationClipIcon == null)
                    _particleSystemAnimationClipIcon = EditorGUIUtility.IconContent("AnimationClip On Icon").image;
                return _particleSystemAnimationClipIcon;
            }
        }
    }
}