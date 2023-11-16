// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using CommonControls.FileTypes.Animation;
using System.Linq;
using AssetManagement.Geometry.DataStructures.Unmanaged;
using AssetManagement.AnimationProcessor;

namespace AssetManagement.Geometry.MeshProcessing.Packed
{
    public class PackedMeshProcessor
    {
        /// <summary>
        /// Checks if the bones in vertex weights exist in the skeleton
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="animationFile"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool CompareMeshVertexWeigtBonesToSkeleton(PackedMesh mesh, AnimationFile animationFile)
        {
            // TODO: TEST THIS        

            if (mesh.VertexWeights.Any(weight => SkeletonHelper.GetIdFromBoneName(animationFile, weight.boneName) == -1))
                return false;

            return true;
        }
        private static bool AlmostEqualToOne(float weight) => weight > 0.95f && weight < 1.05f ? true : false;

        public static void CheckForWeightErrors(PackedMesh outMesh)
        {
            for (var testVertexIndex = 0; testVertexIndex < outMesh.Vertices.Count; testVertexIndex++)
            {
                var testWeight = 0.0f;

                for (var vertexWeightIndex = 0; vertexWeightIndex < outMesh.VertexWeights.Count; vertexWeightIndex++)
                {
                    if (outMesh.VertexWeights[vertexWeightIndex].vertexIndex == testVertexIndex)
                    {
                        testWeight += outMesh.VertexWeights[vertexWeightIndex].weight;
                    }
                }                

                if (!AlmostEqualToOne(testWeight))
                {
                    throw new System.Exception("vertex is null-weighted");
                }
            }
        }

    }
}
