using CommonControls.FileTypes.Animation;
using CommonControls.Services;
using Serilog;
using Assimp;
using CommonControls.FileTypes.RigidModel;
using CommonControls.FileTypes.RigidModel.Vertex;
using System.Linq;
using CommonControls.Common;
using System.Windows;
using Microsoft.Xna.Framework;

namespace CommonControls.AssimpImportExport
{
    public class AssimpExporter
    {
        public static float SCALE_FACTOR = 39.37008f;

        ILogger _logger = Serilog.Log.ForContext<AssimpExporter>();

        private Assimp.Scene _assimpScene;
        private AnimationFile _skeletonFile;
        private PackFileService _packFileService;
        private AssimpAnimationService _animationService;
        public AssimpExporter(PackFileService packFileService)
        {
            _packFileService = packFileService;
            _assimpScene = new Assimp.Scene();
            _assimpScene.Metadata.Add("UnitScaleFactor", new Metadata.Entry(MetaDataType.Float, 2.54f));
            MakeAssimpMaterials();            
        }

        private void MakeAssimpMaterials()
        {
            var defaultMaterial = new Material();
            defaultMaterial.ColorDiffuse = new Assimp.Color4D(1, 0, 0, 1);
            defaultMaterial.ColorSpecular = new Assimp.Color4D(0.1f, 0.1f, 0.1f, 1);
            defaultMaterial.Shininess = 0.1f;                     

            _assimpScene.Materials.Add(defaultMaterial);
        }

        public void ExportRmv2ModelFile(RmvFile soureModel, string path)
        {
            _assimpScene.RootNode = new Assimp.Node();

            var addRiggedMe = MakeSkeleton(soureModel);
            
            // TODO: make sure to not do rigging operation if skeleton load failed
            AddMeshes(soureModel, _animationService.Skeleton);

            var assimpFileService = new AssimpFileService(_packFileService, _assimpScene);
            var animFile = assimpFileService.LoadSkeletonFileByPath(@"animations\battle\humanoid01\sword_and_shield\stand\hu1_sws_combat_idle_01.anim");
            
            _animationService.AddFrames(animFile);


            using (var importer = new AssimpContext())
            {
                importer.ExportFile(_assimpScene, @"C:\temp\as_Skeleton_tester.fbx", "fbx",
                   PostProcessSteps.GlobalScale);
            }

        }

        bool MakeSkeleton(RmvFile soureModel)
        {
            var skeletonName = soureModel.Header.SkeletonName;
            var fileService = new AssimpFileService(_packFileService, _assimpScene);
            _skeletonFile = fileService.LoadSkeletonFileByIdString(skeletonName);

            if (_skeletonFile == null)
            {
                _logger.Here().Warning($"Failed to load skeleton '{skeletonName}', it doesn't exist in memory. Make sure to Load All CA Packs before importing Rigged Models! Or add the appropiate skeleton to your project in 'animations\\skeletons\\'.");
                MessageBox.Show($"\"Failed to load skeleton '{skeletonName}' \rMake sure to Load All CA Packs before importing Rigged Models!\rOr add the appropiate skeleton to your project in 'animations\\\\skeletons\\\\'.\"", "Skeleton Missing Warning");
                return false;
            }

            AddSkeletonIdNode(soureModel.Header.SkeletonName);
            _animationService = new AssimpAnimationService(_skeletonFile, _assimpScene);
            _animationService.MakeSkeleton();

            return true;
        }



        private static void MakeAssimpMeshBones(Mesh asMesh, MSkeleton asSkeleton)
        {
            // -- fills assimpt mesh bones with the needed info
            asMesh.Bones.AddRange(new Bone[asSkeleton.Bones.Length].ToList());
            for (int i = 0; i < asMesh.Bones.Count; i++)
            {
                asMesh.Bones[i] = new Assimp.Bone();
                asMesh.Bones[i].OffsetMatrix = AssimpAnimationService.ToAssimpMatrix4x4(asSkeleton.InverseBindpose[i]);
                asMesh.Bones[i].Name = asSkeleton.Bones[i].Name;
            }
        }

        private static void CopyMeshVertexWeights(Mesh asMesh, RmvMesh rmv2Mesh)        
        {                   
            for (int vertexIndex = 0; vertexIndex < rmv2Mesh.VertexList.Length; vertexIndex++)
            {
                for (int weightIndex = 0; weightIndex < rmv2Mesh.VertexList[vertexIndex].WeightCount; weightIndex++)
                {
                    var boneIndex = rmv2Mesh.VertexList[vertexIndex].BoneIndex[weightIndex];
                    var boneWeight = rmv2Mesh.VertexList[vertexIndex].BoneWeight[weightIndex];

                    asMesh.Bones[boneIndex].VertexWeights.Add(new VertexWeight(vertexIndex, boneWeight));
                }
            }
        }

        private void AddOneBinePoseFrame(MSkeleton asSkeleton)
        {
            // fill 1 frame 
            _assimpScene.Animations.Add(new Animation());
            _assimpScene.Animations[0].NodeAnimationChannels.AddRange(new NodeAnimationChannel[asSkeleton.Bones.Length]);
            _assimpScene.Animations[0].DurationInTicks = 10000;
            _assimpScene.Animations[0].TicksPerSecond = 1000;

            for (int boneIndex = 0; boneIndex < asSkeleton.Bones.Length; boneIndex++)
            {
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex] = new NodeAnimationChannel();
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].NodeName = asSkeleton.Bones[boneIndex].Name;
                var transValue = new Assimp.Vector3D()
                {
                    X = -asSkeleton.Bones[boneIndex].LocalTranslation.X,
                    Y = asSkeleton.Bones[boneIndex].LocalTranslation.Y,
                    Z = asSkeleton.Bones[boneIndex].LocalTranslation.Z,
                };
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].PositionKeys.Add(new VectorKey(0, transValue));
                var quatValue = new Assimp.Quaternion()
                {
                    X = asSkeleton.Bones[boneIndex].LocalRotation.X,
                    Y = -asSkeleton.Bones[boneIndex].LocalRotation.Y,
                    Z = -asSkeleton.Bones[boneIndex].LocalRotation.Z,
                    W = asSkeleton.Bones[boneIndex].LocalRotation.W,
                };
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].RotationKeys.Add(new QuaternionKey(0, quatValue));
            }
        }
     
        /// <summary>
        /// Add meshes to scene from RMV2 File        
        /// </summary>
        /// <param name="soureModel"></param>
        private void AddMeshes(RmvFile soureModel, MSkeleton asSkeleton)
        {
            // Scene-global index used to assign meshes to a scene graph node; references an item in the Scene's List<Mesh>
            var nodeIndexToSceneMesh = 0;

            // Scene-global index used to assign a material to a mesh, references an item in the Scene's List<Material>
            var meshIndexToSceneMaterial = 0;
            foreach (var rmv2Model in soureModel.ModelList[0])
            {
                // setup assimp node
                var meshNode = new Assimp.Node(rmv2Model.Material.ModelName);
                meshNode.MeshIndices.Add(nodeIndexToSceneMesh);
                _assimpScene.RootNode.Children.Add(meshNode);

                // process meshes
                var assimpMesh = GetAssimpMeshFromRmv2Mesh(rmv2Model.Mesh);
                MakeAssimpMeshBones(assimpMesh, asSkeleton);
                CopyMeshVertexWeights(assimpMesh, rmv2Model.Mesh);

                // assign material
                assimpMesh.MaterialIndex = meshIndexToSceneMaterial;
                
                // add meshe to scene
                _assimpScene.Meshes.Add(assimpMesh);

                // mainain global scene mesh index
                nodeIndexToSceneMesh++;
            }
        }

        private Assimp.Mesh GetAssimpMeshFromRmv2Mesh(RmvMesh rmv2Mesh)
        {
            var assimpMesh = new Assimp.Mesh(PrimitiveType.Triangle);
            AllocateVertices(assimpMesh, rmv2Mesh.VertexList.Length);

            for (int vertexIndex = 0; vertexIndex < rmv2Mesh.VertexList.Length; vertexIndex++) // one triangle
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
            _assimpScene.RootNode.Children.Insert(0, new Node("skeleton//" + skeletonOdStriog, _assimpScene.RootNode));
        }

        private Assimp.Node AddSkeletonIdNode(Assimp.Node parent, string skeletonOdStriog)
        {
            var newNode = new Node("skeleton//" + skeletonOdStriog, _assimpScene.RootNode);
            _assimpScene.RootNode.Children.Insert(1, newNode);


            return newNode;
        }

        private void AdSkeletonFromAnimFIle(AnimationFile animSkeleton)
        {

            for (int i = 0; i < animSkeleton.Bones.Length; i++)
            {
                ref var bone = ref animSkeleton.Bones[i];


                var assBone = new Assimp.Bone();
                var assNode = new Assimp.Node();

                // TODO: implement skeleton
                // TODO: maybe use Ole's Node system
                /*
                    algo from making skeleton assimp.Bones
                    assBone =  nwe Bone[N] from animSkeleton

                    run through packed vertices B
                        run through vertex bone index I
                            Bone[vertex.Index[I]].addWeight(vertexIndex, vertex.weights[I]);
                    
                    (Add inverse GLOBAL tranform matrix to "Bone.offsetMatrix")

                    algo for making skeleton NODES
                    make list of nodes[] from skeleton file
                    use 
                    LOOP n over bones.length
                    
                        parentId = anim.bones[n].parentId;
                        Node = from anim.bones[n];
                        nodes[parentId].AddChild(Node);

                     (Add  LOCAL tranform matrix to "Node.transform")

                */


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
            
            destMesh.Vertices[vertexIndex] = new Assimp.Vector3D(-v.Position.X, v.Position.Y, v.Position.Z) * SCALE_FACTOR;
            destMesh.Normals[vertexIndex] = new Assimp.Vector3D(-v.Normal.X, v.Normal.Y, v.Normal.Z);
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
            destMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(v.Uv.X, v.Uv.Y, 0));
            destMesh.Tangents.Add(new Vector3D(v.Tangent.X, v.Tangent.Y, v.Tangent.Z));
            destMesh.BiTangents.Add(new Assimp.Vector3D(v.BiNormal.X, v.BiNormal.Y, v.BiNormal.Z));
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
