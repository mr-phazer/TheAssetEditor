//using Microsoft.Xna.Framework;
//using AssetManagement.Geometry.DataStructures.Unmanaged;
//using AssetManagement.Geometry.Marshalling;

//namespace AssetManagement.Geometry.DataStructures.Managed
//{
//    public class PackCommonVertex : IMarshalable<ExtPackedCommonVertex>
//    {
//        public Vector4 Position { set; get; }
//        public Vector3 Normal { set; get; }
//        public Vector3 Bitangent { set; get; }
//        public Vector3 Tangent { set; get; }
//        public Vector2 Uv { set; get; }
//        public Vector4 Color { set; get; }

//        public override void FillFromStruct(in ExtPackedCommonVertex srcStruct)
//        {
//            Position = FromNativeHelpers.GetVector(srcStruct.Position);

//            Normal = new Vector3(srcStruct.Normal.x, srcStruct.Normal.y, srcStruct.Normal.z);
//            Bitangent = new Vector3(srcStruct.Bitangent.x, srcStruct.Bitangent.y, srcStruct.Bitangent.z);
//            Tangent = new Vector3(srcStruct.Tangent.x, srcStruct.Tangent.y, srcStruct.Tangent.z);

//            Uv = new Vector2(srcStruct.Uv.x, srcStruct.Uv.y);
//        }

//        public override void FillStruct(out ExtPackedCommonVertex destStruct)
//        {
//            destStruct.Position = XMFloatHelper.GetXMFloat(Position);
//            destStruct.Normal = XMFloatHelper.GetXMFloat(Normal);
//            destStruct.Tangent = XMFloatHelper.GetXMFloat(Tangent);
//            destStruct.Bitangent = XMFloatHelper.GetXMFloat(Bitangent);
//            destStruct.Uv = XMFloatHelper.GetXMFloat(Uv);
//            destStruct.Color = XMFloatHelper.GetXMFloat(Color);
//        }
//    }

//    public class VertexWeight
//    {
//        public string BoneName { get; set; }
//        public int BoneIndex { get; set; }
//        public int VertexIndex { get; set; }
//        public float Weight { get; set; }
//    }
//};


