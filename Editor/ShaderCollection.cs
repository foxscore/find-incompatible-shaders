using UnityEngine;

namespace Fox_score.Find_Incompatible_Shaders
{
    // [CreateAssetMenu(fileName = "FILENAME", menuName = "Shader collection", order = 0)]
    public sealed class ShaderCollection : ScriptableObject
    {
        public Shader[] shaders;
    }
}