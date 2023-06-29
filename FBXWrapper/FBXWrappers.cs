using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FBXWrapper.Structs;
using System.Collections.Generic;

namespace FBXWrapper
{
    public class DLLFunctionsFBXSDK
    {
        [DllImport(@"K:\Coding\repos\TheAssetEditor\x64\Debug\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateSceneFBX(IntPtr ptrInstance, string path);


        //[DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?CreateContainer@FBXSCeneContainer@@QEAAPEAV1@XZ")]
        //public static extern IntPtr CreateContainer();

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateFBXContainer();


        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPackedVertices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);


        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetIndices(IntPtr ptrInstances, int meshIndex, out IntPtr vertices, out int itemCount);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetName(IntPtr ptrInstances, int meshIndex);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMeshCount(IntPtr ptrInstances);

        [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\FBXWRapperDllCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddBoneInfo(IntPtr ptrInstances, string boneName);
    }

    public class FBXScenContainerService
    {
        public static PackedCommonVertex[]? GetPackesVertices(IntPtr fbxContainer, int meshIndex)
        {
            IntPtr pVerticesPtr = IntPtr.Zero;
            int length = 0;
            DLLFunctionsFBXSDK.GetPackedVertices(fbxContainer, meshIndex, out pVerticesPtr, out length);

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
            DLLFunctionsFBXSDK.GetIndices(fbxContainer, meshIndex, out pIndices, out length);

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

            IntPtr namePtr = DLLFunctionsFBXSDK.GetName(fbxContainer, meshIndex);
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
            var meshCount = DLLFunctionsFBXSDK.GetMeshCount(fbxSceneContainer);

            for (int i = 0; i < meshCount; i++)
            {
                meshList.Add(GetPackedMesh(fbxSceneContainer, i));
            }

            return meshList;
        }
    }

    public interface IDataWriter
{
    public abstract void DoWriting(string path);
};

public class DataWriterText : IDataWriter // impl for text
{
    public virtual void DoWriting(string path) { /* write text;*/ }
}

public class DataWriterFactory
{
    enum FileTypeEnum { Text, Binary, Format1, Format2}
    Dictionary<FileTypeEnum, IDataWriter> fileTypeWriters = new Dictionary<FileTypeEnum, IDataWriter>();//should be init with support writers
    IDataWriter Create(string file, FileTypeEnum type) { return fileTypeWriters[type]; }
}

public class DataWriterClient
{
    IDataWriter Writer { get; set; } = new DataWriterText(); // default writer?
    void Write(string file) { Writer.DoWriting(file); }        
}

        




}