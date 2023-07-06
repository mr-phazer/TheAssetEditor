using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommonControls.Services;
using CommonControls.FileTypes.Animation;

namespace CommonControls.ModelFiles.FBX
{
    public class SceneImorterService
    {
        public static SceneContainer CreateSceneFromFBX(string fileName, PackFileService pfs, out string skeletonName)
        {
            var newSceneContainer = new SceneContainer();

            var fbxSceneLoader = FBXSeneLoaderServiceDLL.CreateSceneFBX(fileName); 
            FillBoneTable(fbxSceneLoader, out skeletonName, pfs); // have to send bone table before mesh processing

            var ptrNativeSceneContainer = FBXSeneLoaderServiceDLL.ProcessAndFillScene(fbxSceneLoader);
            SceneMarshaller.ToManaged(ptrNativeSceneContainer, newSceneContainer);

            FBXSeneLoaderServiceDLL.DeleteBaseObj(fbxSceneLoader);

            return newSceneContainer;
        }

        private static void FillBoneTable(IntPtr fbxSceneLoader, out string outSkeletonName, PackFileService pfs)
        {
            var skeletonNamePtr = FBXSeneLoaderServiceDLL.GetSkeletonNameFromScene(fbxSceneLoader);

            outSkeletonName = "";

            if (skeletonNamePtr != null)
            {
                string skeletonName = Marshal.PtrToStringUTF8(skeletonNamePtr);
                if (skeletonName == null)
                {
                    throw new Exception("marshalling error");
                }
                else if (skeletonName == "")
                {
                    return;
                }

                string skeletonPath = $"animations/skeletons/{skeletonName}.anim";
                var packFile = pfs.FindFile(skeletonPath);

                if (packFile == null)
                {
                    throw new Exception("packFile==null when loading skeleton");
                }

                var animFile = AnimationFile.Create(packFile.DataSource.ReadDataAsChunk());

                if (animFile != null)
                {
                    FBXSeneLoaderServiceDLL.ClearBoneNames(fbxSceneLoader);

                    foreach (var bone in animFile.Bones)
                        FBXSeneLoaderServiceDLL.AddBoneName(fbxSceneLoader, bone.Name, bone.Name.Length);

                    outSkeletonName = skeletonName; // output a skeleton name only when a skeleton is sucessfully loaded
                }
            };
        }
    }
    public class SceneMarshaller
    {
        public static void ToManaged(IntPtr ptrFbxSceneContainer, SceneContainer destSceneContainer)
        {
            destSceneContainer.Meshes = GetAllPackedMeshes(ptrFbxSceneContainer);
            /*
            destScene.Bones = GetAllBones();
            destScene.Animations = GetAllAnimations();
            destScene.RootNode = GetNodeHierachy()
            etc...
            */
        }

        public void AddSkeletonFromFile(string path)
        {

        }

        public static PackedCommonVertex[]? GetPackesVertices(IntPtr fbxContainer, int meshIndex)
        {
            IntPtr pVerticesPtr = IntPtr.Zero;
            int length = 0;
            FBXSCeneContainerDll.GetPackedVertices(fbxContainer, meshIndex, out pVerticesPtr, out length);

            if (pVerticesPtr == IntPtr.Zero || length == 0)
            {
                return null;
            }

            PackedCommonVertex[] data = new PackedCommonVertex[length];
            for (int vertexIndex = 0; vertexIndex < length; vertexIndex++)
            {
                var ptr = Marshal.PtrToStructure(pVerticesPtr + vertexIndex * Marshal.SizeOf(typeof(PackedCommonVertex)), typeof(PackedCommonVertex));

                if (ptr != null)
                    data[vertexIndex] = (PackedCommonVertex)ptr;
            }

            return data;
        }

        public static ushort[]? GetIndices(IntPtr fbxContainer, int meshIndex)
        {
            IntPtr pIndices = IntPtr.Zero;
            int length = 0;
            FBXSCeneContainerDll.GetIndices(fbxContainer, meshIndex, out pIndices, out length);

            if (pIndices == IntPtr.Zero || length == 0)
                return null;

            var indexArray = new ushort[length];

            for (int indicesIndex = 0; indicesIndex < length; indicesIndex++)
            {
                indexArray[indicesIndex] = (ushort)Marshal.PtrToStructure(pIndices + indicesIndex * Marshal.SizeOf(typeof(ushort)), typeof(ushort));
            }
            return indexArray;
        }

        public static PackedMesh GetPackedMesh(IntPtr fbxContainer, int meshIndex)
        {
            var indices = GetIndices(fbxContainer, meshIndex);
            var vertices = GetPackesVertices(fbxContainer, meshIndex);

            IntPtr namePtr = FBXSCeneContainerDll.GetMeshName(fbxContainer, meshIndex);
            var tempName = Marshal.PtrToStringUTF8(namePtr);

            if (vertices == null || indices == null || tempName == null)
                throw new Exception("Params/Input Data Invalid: Vertices, Indices or Name == null");

            PackedMesh packedMesh = new PackedMesh();
            packedMesh.Vertices = new List<PackedCommonVertex>();
            packedMesh.Indices = new List<ushort>();
            packedMesh.Vertices.AddRange(vertices);
            packedMesh.Indices.AddRange(indices);
            packedMesh.Name = tempName;

            return packedMesh;
        }

        static public List<PackedMesh> GetAllPackedMeshes(IntPtr fbxSceneContainer)
        {
            List<PackedMesh> meshList = new List<PackedMesh>();
            var meshCount = FBXSCeneContainerDll.GetMeshCount(fbxSceneContainer);

            for (int i = 0; i < meshCount; i++)
            {
                meshList.Add(GetPackedMesh(fbxSceneContainer, i));
            }

            return meshList;
        }

    }

    // TODO: move to scene.cs
    public class FBXSCeneContainerDll
    {
        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPackedVertices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetIndices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMeshCount(IntPtr ptrInstances);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMeshName(IntPtr ptrInstances, int meshIndex);
    }

    public class FBXSeneLoaderServiceDLL
    {
        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteBaseObj(IntPtr ptrInstances);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ProcessAndFillScene(IntPtr ptrFBXSceneLoaderService);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSceneFBX(string path);

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateFBXSceneImporterService();

        [DllImport(@"FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateFBXContainer();

        [DllImport("FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSkeletonNameFromScene(IntPtr ptrSceneLoader);

        [DllImport("FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AddBoneName(IntPtr ptrInstances, string boneName, int len);

        [DllImport("FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearBoneNames(IntPtr ptrInstances);
    }
}
