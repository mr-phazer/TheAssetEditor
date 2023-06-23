using View3D.Rendering.Geometry;
using Simplygon;
using Serilog;
using CommonControls.Common;

namespace View3D.Services.MeshOptimization
{
    public class MeshOptimizerService 
    {
        ILogger _logger = Logging.Create<MeshOptimizerService>();
        public MeshObject CreatedReducedCopy(MeshObject originalMesh, float factor)
        {
            MeshObject newMesh = originalMesh.Clone(false);
            using (ISimplygon sg = Loader.InitSimplygon(out var simplygonErrorCode, out var simplygonErrorMessage))
            {                
                if (simplygonErrorCode == EErrorCodes.NoError)
                {
                    return SimplygonMeshOptimizer.GetReducedMeshCopy(sg, originalMesh, factor);
                }
                else // simplygon failed to initialize, use old mesh reducer (MeshDecimator)
                {
                    _logger.Warning(@"Simplygon load failed with {simplygonErrorCode.ToString()}");
                    return DecimatorMeshOptimizer.GetReducedMeshCopy(originalMesh, factor);
                }
            }
        }
    }

}
