// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AssetManagement.AssetBuilders;
using CommonControls.Services;
using CommonControls.Interfaces.AssetManagement;

namespace AssetManagement.Geometry.Strategies.Fbx.FbxAssetHandling
{
    public class FbxAssetExporter : IAssetExporter
    {
        public string[] Formats => new string[] { ".fbx" };

        private readonly PackFileService _packFileService;

        public FbxAssetExporter(PackFileService pfs)
        {            
            _packFileService = pfs;
        }

        // TODO: what todo about return value not sure if the FBX SDK CAN return a binary FBX file in memory
        public byte[] ExportAsset(AssetManagerData inputData, AssetConfigData configData)
        {
            // TODO: place the export dialog here? Or "Outside"?
            // TODO: MAKE: ExportDialogViewModel.ShowDialog(inputData, configData);                       

            var sceneBuilder = new SimpleSceneBuilder();
            sceneBuilder.AddSkeleton(inputData.skeletonFile);
            sceneBuilder.AddMeshes(inputData.RigidModelFile);

            var sceneExporter = new SceneExporter();
            sceneExporter.ExportScene(sceneBuilder.CurrentSceneContainer, inputData.DestinationPath);

            // TODO: Q: Do I need to return FBX as binary data ALSO, when "SaveScene()" stores it on disk?            
            return null; 
        }
    }

}
