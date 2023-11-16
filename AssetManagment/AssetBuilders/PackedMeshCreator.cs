using System.Linq;
using AssetManagement.Geometry.DataStructures.Unmanaged;
using CommonControls.FileTypes.Animation;
using CommonControls.FileTypes.RigidModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AssetManagement.AssetBuilders
{

    public interface IPackedMeshBuilder
    {
        public PackedMesh CreateMesh(RmvModel model);
        public List<PackedMesh> CreateMeshList(RmvFile file);
    }

    internal class BasicPackedMeshBuilder : IPackedMeshBuilder
    {
        public virtual PackedMesh CreateMesh(RmvModel model)
        {
            return Rmv2ModelToPackedMesh(model);
        }

        public virtual List<PackedMesh> CreateMeshList(RmvFile file)
        {
            var outPackedMeshes = new List<PackedMesh>();
            foreach (var model in file.ModelList[0])
            {
                outPackedMeshes.Add(CreateMesh(model));
            }
            return outPackedMeshes;
        }

        public PackedMesh Rmv2ModelToPackedMesh(RmvModel inMmodel)
        {
            var outMesh = new PackedMesh() { Name = inMmodel.Material.ModelName, };          
                        
            outMesh.Vertices = inMmodel.Mesh.VertexList.Select(vertex => PackedVertexHelper.CreateExtPackedVertex(vertex)).ToList();
            outMesh.Indices = inMmodel.Mesh.IndexList.Select(index => (uint)index).ToList();

            return outMesh;
        }
    }

    internal class WeightedIndexedPackedMeshBuilder : BasicPackedMeshBuilder
    {
        readonly AnimationFile _skeletonFile;

        public WeightedIndexedPackedMeshBuilder(AnimationFile skeletonFile)
        {
            _skeletonFile = skeletonFile;
        }

        override public PackedMesh CreateMesh(RmvModel InModel)
        {
            var outMesh = Rmv2ModelToPackedMesh(InModel);

            AddVertexWeights(InModel, outMesh);

            return outMesh;
        }

        public void AddVertexWeights(RmvModel inModel, PackedMesh outMesh)
        {
            var weightcreator = new VertexWeightCreator(inModel, _skeletonFile);
            outMesh.VertexWeights = weightcreator.CreateVertexWeigts();
        }
    }

    public class WeightedPackedMeshBuilder
    {
        static public List<PackedMesh> BuildMeshes(RmvFile rmv2File, AnimationFile skeletonFie)
        {
            var meshBuilder = new BasicPackedMeshBuilder();
            var meshes = meshBuilder.CreateMeshList(rmv2File);           

            MeshWeightingCreator.AddWeighting(meshes, rmv2File, skeletonFie);

            return meshes;
        }
    }

    public class MeshWeightingCreator  
    {            
        public static void AddWeighting(List<PackedMesh> inoutMeshes, RmvFile rmv2File, AnimationFile skeletonFie)
        {
            if (skeletonFie == null)
                return;

            for (int i = 0; i < rmv2File.ModelList[0].Length; i++)
            {
                CreateWeights(rmv2File.ModelList[0][i], inoutMeshes[i], skeletonFie);
            }
        }

        static private void CreateWeights(RmvModel inModel, PackedMesh outMesh, AnimationFile skeletonFile)
        {
            var weightcreator = new VertexWeightCreator(inModel, skeletonFile);
            outMesh.VertexWeights = weightcreator.CreateVertexWeigts();
        }

    }


    public class PackedMeshBuilderFactory
    {
        public static IPackedMeshBuilder GetBuilder(AnimationFile skeletonFile)
        {
            if (skeletonFile == null)
            {
                return new BasicPackedMeshBuilder();
            }
            else
            {
                return new WeightedIndexedPackedMeshBuilder(skeletonFile); ;

            }
        }
    }
}
