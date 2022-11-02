using UnityEngine;

namespace Fox_score.Find_Incompatible_Shaders
{
    public static class ShaderCollectionLoader
    {
        private static ShaderCollection _vrcShaderNames;
        private static ShaderCollection _vrcParticleShaderNames;

        public static ShaderCollection VrcShaders
        {
            get
            {
                if (_vrcShaderNames == null)
                    _vrcShaderNames = Resources.Load<ShaderCollection>("Foxy/VrchatMobileShaders");
                return _vrcShaderNames;
            }
        }
        
        public static ShaderCollection VrcParticleShaders
        {
            get
            {
                if (_vrcParticleShaderNames == null)
                    _vrcParticleShaderNames = Resources.Load<ShaderCollection>("Foxy/VrchatMobileParticleShaders");
                return _vrcParticleShaderNames;
            }
        }
    }
}