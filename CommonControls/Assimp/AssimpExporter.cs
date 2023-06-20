using CommonControls.FileTypes.Animation;
using CommonControls.Services;
using Serilog;
using Assimp;
using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.RigidModel.Vertex;
using Assimp.Unmanaged;
using Assimp;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Linq;

namespace CommonControls.ModelImportExport
{
    public class AssimpExporter
    {
        ILogger _logger = Serilog.Log.ForContext<AssimpExporter>();

        private Assimp.Scene _asssimpScene;
        private AnimationFile _skeletonFile;
        private PackFileService _packFileService;

        public AssimpExporter()
        {
            _asssimpScene = new Assimp.Scene();
        }

        public void ExportModelFile(RmvFile soureModel, string path)
        {

            _asssimpScene.RootNode = new Assimp.Node();
            AddSkeletonIdNode("humanoid01");

            //_asssimpScene.RootNode.Children.Add(new Assimp.Node("Test"));
            //_asssimpScene.RootNode.Children.Add(new Assimp.Node("Test1"));
            //_asssimpScene.RootNode.Children.Add(new Assimp.Node("Test2"));

            ProcessMeshes(soureModel);

            using (var importer = new AssimpContext())
            {
                importer.ExportFile(_asssimpScene, @"C:\temp\as_tester.fbx", "fbx");

            }
        }

        private void ProcessMeshes(RmvFile soureModel)
        {
            var mesSceneMeshIndex = 0;
            foreach (var imesh_lod0 in soureModel.ModelList[0])
            {
                var meshNode = new Assimp.Node(imesh_lod0.Material.ModelName);
                _asssimpScene.RootNode.Children.Add(meshNode);

                // make assimp mesh from rmv2 data
                var assimpMesh = Rmv2MeshToAsimpMesh(imesh_lod0.Mesh);
                _asssimpScene.Meshes.Add(assimpMesh);

                // make sure the internal mesh indices fit
                _asssimpScene.RootNode.MeshIndices.Add(mesSceneMeshIndex);

                mesSceneMeshIndex++;
            }
        }

        private Assimp.Mesh Rmv2MeshToAsimpMesh(RmvMesh rmv2Mesh)
        {
            var assimpMesh = new Assimp.Mesh(PrimitiveType.Triangle);
            //AllocateVertices(assimpMesh, rmv2Mesh.VertexList.Length);

            for (int vertexIndex = 0; vertexIndex < rmv2Mesh.VertexList.Length ; vertexIndex++) // one triangle
                RMv2PackedVertexToAssimpVertex(rmv2Mesh.VertexList[vertexIndex], assimpMesh, vertexIndex);                          

            for (int triangleIndex = 0; triangleIndex < rmv2Mesh.IndexList.Length / 3; triangleIndex++) // one triangle
            {
                var c0 = rmv2Mesh.IndexList[(3 * triangleIndex)];
                var c1 = rmv2Mesh.IndexList[(3 * triangleIndex) + 1];
                var c2 = rmv2Mesh.IndexList[(3 * triangleIndex + 2)];

                var newFace = new Face(new int[] { c0, c1, c2 });

                //newFace.Indices.Add(c0);
                //newFace.Indices.Add(c1);
                //newFace.Indices.Add(c2);
                assimpMesh.Faces.Add(newFace);
            }

                
                

            return assimpMesh;
        }



        private void AddSkeletonIdNode(string skeletonOdStriog)
        {
            _asssimpScene.RootNode.Children.Insert(0, new Node("skeleton//" + skeletonOdStriog, _asssimpScene.RootNode));
        }

        private Assimp.Node AddSkeletonIdNode(Assimp.Node parent, string skeletonOdStriog)
        {
            var newNode = new Node("skeleton//" + skeletonOdStriog, _asssimpScene.RootNode);
            _asssimpScene.RootNode.Children.Insert(1, newNode);
                        
            return newNode;
        }


        private void AdSkeletonFromAnimFIle(AnimationFile animSkeleton)
        {

            for (int i = 0; i < animSkeleton.Bones.Length; i++)
            {
                ref var bone = ref animSkeleton.Bones[i];



                //if (bone.Id == AnimationFile.BoneInfo.NO_PARENT_ID)
                {

                    //AddSkeletonIdNode


                }



            }


        }


        //private void AdSkeletonFromAnimFIle(AnimationFile animSkeleton)

        //private void Add(RmvFile sourceFMRV2)
        //{


        //    _assScene.Meshes.Materials.Add(sourceFMRV2);
        //    _assScene.Materials[0] = null;
        //    _assScene.NumMaterials = 1;
        //    _assScene.Materials[0]

        //    _assScene.mMeshes = new aiMesh*[1];
        //    _assScene.mMeshes[0] = nullptr;
        //    _assScene.mNumMeshes = 1;

        //    _assScene.mMeshes[0] = new aiMesh();
        //    _assScene.mMeshes[0]->mMaterialIndex = 0;

        //    _assScene.mRootNode->mMeshes = new unsigned int[1];
        //    _assScene.mRootNode->mMeshes[0] = 0;
        //    _assScene.mRootNode->mNumMeshes = 1;


        //}

        private void AllocateVertices(Assimp.Mesh destMesh, int count)
        {
            destMesh.Vertices.AddRange(new Vector3D[count].ToList());
            destMesh.Normals.AddRange(new Vector3D[count].ToList());
            destMesh.TextureCoordinateChannels[0].AddRange(new Vector3D[count].ToList());
            destMesh.Tangents.AddRange(new Vector3D[count].ToList());
            destMesh.BiTangents.AddRange(new Vector3D[count].ToList());                     
        }

        /// <summary>
        /// Copy the values of packed vertex, into simp Values
        /// </summary>
        private void RMv2PackedVertexToAssimpVertex(
            CommonVertex v,
            Assimp.Mesh destMesh,
            int vertexIndex)
        {
            destMesh.Vertices[vertexIndex] = new Assimp.Vector3D(v.Position.X, v.Position.Y, v.Position.Z);
            destMesh.Normals[vertexIndex] = new Assimp.Vector3D(v.Normal.X, v.Normal.Y, v.Normal.Z);
            destMesh.TextureCoordinateChannels[0][vertexIndex] = new Assimp.Vector3D(v.Uv.X, v.Uv.Y, 0);
            destMesh.Tangents[vertexIndex] = new Assimp.Vector3D(v.Tangent.X, v.Tangent.Y, v.Tangent.Z);
            destMesh.BiTangents[vertexIndex] = new Assimp.Vector3D(v.BiNormal.X, v.BiNormal.Y, v.BiNormal.Z);
        }
        private void AddRMv2PackedVertexToAssimpVertex(
            CommonVertex v,
            Assimp.Mesh destMesh,
            int vertexIndex)
        {
            destMesh.Vertices.Add(new Assimp.Vector3D(v.Position.X, v.Position.Y, v.Position.Z));
            destMesh.Normals.Add(new Assimp.Vector3D(v.Normal.X, v.Normal.Y, v.Normal.Z));
            destMesh.TextureCoordinateChannels[0][vertexIndex].Add(Assimp.Vector3D(v.Uv.X, v.Uv.Y, 0));
            destMesh.Tangents[vertexIndex] = new Assimp.Vector3D(v.Tangent.X, v.Tangent.Y, v.Tangent.Z);
            destMesh.BiTangents[vertexIndex] = new Assimp.Vector3D(v.BiNormal.X, v.BiNormal.Y, v.BiNormal.Z);
        }


        private Assimp.Mesh GetAsAssimpFromRMV2Mesh(RmvMesh inputMesh)
        {
            var newMesh = new Assimp.Mesh(Assimp.PrimitiveType.Triangle);

            for (int vertexIndex = 0; vertexIndex < inputMesh.VertexList.Length; vertexIndex++)
            {
                RMv2PackedVertexToAssimpVertex(inputMesh.VertexList[vertexIndex], newMesh, vertexIndex);
            }


            return newMesh;
        }






    }
}
