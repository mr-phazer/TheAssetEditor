
using AssetManagement.Geometry.DataStructures.Unmanaged;
using AssetManagement.Geometry.Strategies.Fbx.DllDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssetManagement.Geometry.Marshalling
{
    public class SceneMarshallerToManaged
    {
        public static SceneContainer ToManaged(IntPtr ptrFbxSceneContainer)
        {
            var newScene = new SceneContainer();
            newScene.Meshes = GetAllPackedMeshes(ptrFbxSceneContainer);
            newScene.SkeletonName = GetSkeletonNameFromSceneContainer(ptrFbxSceneContainer);
            return newScene;
            /*
            - destScene.Bones = GetAllBones();
            - destScene.Animations = GetAllBones();
            - etc, comming soon
            */
        }

        public static ExtPackedCommonVertex[] GetVertices(IntPtr fbxContainer, int meshIndex)
        {
            var pVerticesPtr = IntPtr.Zero;
            var length = 0;
            pVerticesPtr = FBXSceneContainerDll.GetVertices(fbxContainer, meshIndex, out length);

            if (pVerticesPtr == IntPtr.Zero || length == 0)
            {
                return null;
            }

            var data = new ExtPackedCommonVertex[length];
            for (var vertexIndex = 0; vertexIndex < length; vertexIndex++)
            {
                var ptr = Marshal.PtrToStructure(pVerticesPtr + vertexIndex * Marshal.SizeOf(typeof(ExtPackedCommonVertex)), typeof(ExtPackedCommonVertex));

                if (ptr != null)
                    data[vertexIndex] = (ExtPackedCommonVertex)ptr;
            }

            return data;
        }

        public static uint[] GetIndices(IntPtr fbxContainer, int meshIndex)
        {
            var pIndices = IntPtr.Zero;
            var length = 0;
            pIndices = FBXSceneContainerDll.GetIndices(fbxContainer, meshIndex, out length);

            if (pIndices == IntPtr.Zero || length == 0)
                return null;

            var indexArray = new uint[length];

            for (var indicesIndex = 0; indicesIndex < length; indicesIndex++)
            {
                indexArray[indicesIndex] = (uint)Marshal.PtrToStructure(pIndices + indicesIndex * Marshal.SizeOf(typeof(uint)), typeof(uint));
            }
            return indexArray;
        }

        public static PackedMesh GetPackedMesh(IntPtr fbxContainer, int meshIndex)
        {
            var indices = GetIndices(fbxContainer, meshIndex);
            var vertices = GetVertices(fbxContainer, meshIndex);

            var namePtr = FBXSceneContainerDll.GetMeshName(fbxContainer, meshIndex);
            var tempName = Marshal.PtrToStringUTF8(namePtr);

            if (vertices == null || indices == null || tempName == null)
                throw new Exception("Params/Input Data Invalid: Vertices, Indices or Name == null");

            var packedMesh = new PackedMesh()
            {
                Name = tempName,
                Vertices = vertices.ToList(),
                Indices = indices.ToList(),
                VertexWeights = GetVertexWeights(fbxContainer, meshIndex).ToList(),
            };

            return packedMesh;
        }

        public static ExtVertexWeight[] GetVertexWeights(IntPtr fbxContainer, int meshIndex)
        {
            FBXSceneContainerDll.GetVertexWeights(fbxContainer, meshIndex, out var vertexWeightsPtr, out var weightCount);

            if (vertexWeightsPtr == IntPtr.Zero || weightCount == 0)
            {
                return new ExtVertexWeight[0];
            }

            var data = new ExtVertexWeight[weightCount];
            for (var iWeight = 0; iWeight < weightCount; iWeight++)
            {
                var ptr = Marshal.PtrToStructure(vertexWeightsPtr + iWeight * Marshal.SizeOf(typeof(ExtVertexWeight)), typeof(ExtVertexWeight));


                if (ptr == null)
                {
                    throw new Exception("Fatal Error: ptr == null");
                }
                data[iWeight] = (ExtVertexWeight)ptr;
            }

            return data;
        }

        static public List<PackedMesh> GetAllPackedMeshes(IntPtr fbxSceneContainer)
        {
            var meshList = new List<PackedMesh>();
            var meshCount = FBXSceneContainerDll.GetMeshCount(fbxSceneContainer);

            for (var i = 0; i < meshCount; i++)
            {
                meshList.Add(GetPackedMesh(fbxSceneContainer, i));
            }
            return meshList;
        }

        static public string GetSkeletonNameFromSceneContainer(IntPtr ptrFbxSceneContainer)
        {
            var skeletonNamePtr = FBXSceneContainerDll.GetSkeletonName(ptrFbxSceneContainer);

            if (skeletonNamePtr == IntPtr.Zero)
                return "";

            var skeletonName = Marshal.PtrToStringUTF8(skeletonNamePtr);

            if (skeletonName == null)
                return "";

            return skeletonName;
        }
    }
}
