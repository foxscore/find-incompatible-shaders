using System;
using Object = UnityEngine.Object;

namespace Fox_score.Find_Incompatible_Shaders
{
    [Serializable]
    public class MaterialUsage
    {
        public Object target;
        public MaterialUsageType type;
        
        public string DisplayName => target.name;
        
        public MaterialUsage(Object target, MaterialUsageType type)
        {
            this.target = target;
            this.type = type;
        }
    }
}