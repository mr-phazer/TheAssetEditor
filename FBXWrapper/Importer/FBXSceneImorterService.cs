using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Services;
using CommonControls.Common;
using Serilog;
using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Assimp;
using CommonControls.FileTypes.Animation;
using Filetypes.ByteParsing;
using System.Linq;
using static CommonControls.Editors.AnimationPack.Converters.AnimationBinFileToXmlConverter;
//using CommonControls.ModelFiles;

namespace CommonControls.ModelFiles
{
    public class FBXSCeneContainerDll
    {
        [DllImport(@"K:\Coding\repos\TheAssetEditor\x64\Debug\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateSceneFBX(IntPtr ptrInstance, string path);
                
        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateFBXContainer();

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPackedVertices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetIndices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSkeletonNameFromSceneNodes(IntPtr ptrInstances);
       
        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetName(IntPtr ptrInstances, int meshIndex);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMeshCount(IntPtr ptrInstances);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern void AdddBoneInfo(IntPtr ptrInstances, System.String[] boneName, int len);
    }

    public class FBXSceneImorterService
    {

        private IntPtr _fbxcSceneContainerDLL = IntPtr.Zero;
        

        private FBXSceneImorterService() { }

        public static FBXSceneImorterService Create(string fileName, PackFileService pfs)
        {
            FBXSceneImorterService newInstance = new FBXSceneImorterService();
            newInstance._fbxcSceneContainerDLL = FBXSCeneContainerDll.CreateFBXContainer();
            FBXSCeneContainerDll.CreateSceneFBX(newInstance._fbxcSceneContainerDLL, fileName);


            var skeletonNamePtr = FBXSCeneContainerDll.GetSkeletonNameFromSceneNodes(newInstance._fbxcSceneContainerDLL);

            if (skeletonNamePtr != null)
            {
                string? skeletonName = Marshal.PtrToStringUTF8(skeletonNamePtr);
                string skeletonPath = $"animations/skeletons/{skeletonName}.anim";
                var packFile = pfs.FindFile(skeletonPath);


                var animFIle = AnimationFile.Create(packFile.DataSource.ReadDataAsChunk());

               var animBoneNames = animFIle.Bones.Select(x => x.Name).ToArray();

               unsafe
                {
                    foreach (var bone in animFIle.Bones)
                    {
                        FBXSCeneContainerDll.AdddBoneInfo(newInstance._fbxcSceneContainerDLL, animBoneNames, animBoneNames.Length);
                    }
                }
            }



            return newInstance;
        }


        public string GetSkeletonFromSceneId()
        {

            IntPtr namePtr = FBXSCeneContainerDll.GetSkeletonNameFromSceneNodes();

            string skeletonName = "";
            if (namePtr != IntPtr.Zero)
            {
                skeletonName = Marshal.PtrToStringUTF8(namePtr);
                return skeletonName;
            }

            return "";


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

            IntPtr namePtr = FBXSCeneContainerDll.GetName(fbxContainer, meshIndex);
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
}
