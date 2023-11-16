using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.Animation;
using System.Collections.Generic;
using AssetManagement.Geometry.DataStructures.Unmanaged;
using System;
using AssetManagement.AnimationProcessor;

namespace AssetManagement.AssetBuilders
{
    interface ISceneBuilder
    {
        /// <summary>
        /// Adds a skeleton, limited to 1 per scene for now
        /// </summary>        
        void AddSkeleton(AnimationFile skeletonFile);

        /// <summary>
        /// Adds 1 RMV2 MESH file to scene        
        /// </summary>    
        void AddMesh(RmvModel rmv2Mesh);
        
        /// <summary>
        /// Adds all meshes at LOD 0 from an RMV2 File
        /// </summary>    
        void AddMeshes(RmvFile rmv2File);

        /// <summary>        
        /// Extracts/Decodes the frame data from 1 .ANIM file and adds it to scene
        /// limited to 1 per scene for now
        /// </summary>    
        void AddAnimation(AnimationFile animationFile);
    }

    public class SimpleSceneBuilder : ISceneBuilder
    {
        private AnimationFile _skeletonFile = null;        
        public SceneContainer CurrentSceneContainer { get; private set; } =  new SceneContainer();

        public void AddMesh(RmvModel inputRMV2Mesh)
        {
            var packedMeshBuilder = PackedMeshBuilderFactory.GetBuilder(_skeletonFile);            

            var mesh = packedMeshBuilder.CreateMesh(inputRMV2Mesh);

            CurrentSceneContainer.Meshes.Add(mesh);
        }

        public void AddMeshes(RmvFile inputRMV2File)
        {
            var packedMeshBuilder = PackedMeshBuilderFactory.GetBuilder(_skeletonFile);

            var meshList = packedMeshBuilder.CreateMeshList(inputRMV2File);

            CurrentSceneContainer.Meshes.AddRange(meshList);
            CurrentSceneContainer.SkeletonName = inputRMV2File.Header.SkeletonName;
        }
                
        public void AddSkeleton(AnimationFile skeletonFile)
        {            
            if (skeletonFile == null || !SkeletonHelper.IsFileSkeleton(skeletonFile))
            {
                // need actual skeleton .ANIM to build the bindpose, etc
                return;
            }

            _skeletonFile = skeletonFile;
            CopyBones(skeletonFile);
        }

        public void AddAnimation(AnimationFile animationFile)
        {            
            throw new NotImplementedException("Todo");
            // ... coming soon
        }

        private void CopyBones(AnimationFile skeletonFile)     
        {         
            CurrentSceneContainer.Bones = new List<ExtBoneInfo>();

            for (int boneIndex = 0; boneIndex < skeletonFile.Bones.Length; boneIndex++)
            {
                var newBone = new ExtBoneInfo()
                {
                    id = skeletonFile.Bones[boneIndex].Id,
                    parentId = skeletonFile.Bones[boneIndex].ParentId,
                    name = skeletonFile.Bones[boneIndex].Name,
                    localTranslation = SceneBuilderHelpers.GetBoneTranslation(skeletonFile, boneIndex),
                    localRotation = SceneBuilderHelpers.GetBoneRotation(skeletonFile, boneIndex),
                };

                CurrentSceneContainer.Bones.Add(newBone);
            };
        }              
    }
}
