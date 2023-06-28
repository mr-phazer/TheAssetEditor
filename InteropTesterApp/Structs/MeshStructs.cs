//using PtrFBXContainter = System.IntPtr;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FBXWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FBXControlPoint
    {
        public double x;
        public double Y;
        public double z;
        public double w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMFLOAT4
    {
        float x;
        float y;
        float z;
        float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMFLOAT3
    {
        float x;
        float y;
        float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMFLOAT2
    {
        float x;
        float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoneKey
    {
        XMFLOAT3 translation;
        XMFLOAT4 quaternion;
        double timeStampe;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct VertexInfluence
    {
        uint boneIndex;
        float weight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PackedCommonVertex
    {
        XMFLOAT4 Position;
        XMFLOAT3 Normal;
        XMFLOAT3 BiNormal;
        XMFLOAT3 Tangent;
        XMFLOAT2 Uv;
        XMFLOAT4 Color;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 4)]
        VertexInfluence[] influences; // fixed array length 4        

        int WeightCount;
    };

    struct PackedMesh
    {
        public string Name { set; get; }
        public List<PackedCommonVertex> Vertices { set; get; }
        public List<ushort> Indices { set; get; }        
    }

};




