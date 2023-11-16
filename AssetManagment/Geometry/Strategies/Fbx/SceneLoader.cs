using System;
using AssetManagement.Geometry.DataStructures.Unmanaged;
using AssetManagement.Geometry.Marshalling;
using AssetManagement.Geometry.Strategies.Fbx.DllDefinitions;
using CommonControls.Common;

using Serilog;

namespace AssetManagement.Geometry.Strategies.Fbx
{
    public class SceneLoader
    {
        public static SceneContainer LoadScene(string fileName)
        {
            var fbxSceneLoader = IntPtr.Zero;

            try
            {
                fbxSceneLoader = FBXSeneImporterServiceDLL.CreateSceneFBX(fileName);
                var ptrNativeScene = FBXSeneImporterServiceDLL.ProcessAndFillScene(fbxSceneLoader);
                var newSceneContainter = SceneMarshaller.CopyToManaged(ptrNativeScene);

                return newSceneContainter;
            }
            finally
            {
                if (fbxSceneLoader != IntPtr.Zero)
                    FBXSeneImporterServiceDLL.DeleteBaseObj(fbxSceneLoader);
            }
        }
    }

    // TODO: move to own .cs file??
    public class SceneExporter
    {
        private readonly ILogger _logger = Logging.Create<SceneExporter>();

        public void ExportScene(SceneContainer sourceScene, string fileName)
        {
            var ptrNativeExporter = IntPtr.Zero;
            var ptrNativeceneContainer = IntPtr.Zero;

            _logger.Here().Information($"Exporting geometry to {fileName}, starting FBX SDK service..");

            try
            {
                ptrNativeExporter = FBXSeneExporterServiceDLL.MakeEmptyExporter();
                ptrNativeceneContainer = FBXSeneExporterServiceDLL.GetNativeSceneContainer(ptrNativeExporter);

                SceneMarshaller.CopyToNative(ptrNativeceneContainer, sourceScene);
                FBXSeneExporterServiceDLL.SaveToDisk(ptrNativeExporter, fileName);
            }           
            finally
            {
                if (ptrNativeExporter != IntPtr.Zero)
                    FBXSeneImporterServiceDLL.DeleteBaseObj(ptrNativeExporter);
            }

        }
    }
}
