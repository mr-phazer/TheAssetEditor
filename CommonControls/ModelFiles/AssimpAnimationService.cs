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
using static CommonControls.Editors.AnimationPack.Converters.AnimationBinFileToXmlConverter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using CommonControls.FileTypes.MetaData.Definitions;
using static CommonControls.FileTypes.Animation.AnimationFile;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using SharpDX.Direct3D9;

namespace CommonControls.ModelFiles
{
    public class AssimpAnimationService
    {
        readonly private AnimationFile _bindPoseFile;
        readonly private Assimp.Scene _assimpScene;
        public MSkeleton Skeleton { get; private set; }

        public AssimpAnimationService(AnimationFile bindposeFile, Assimp.Scene asDestScene)
        {
            _bindPoseFile = bindposeFile;
            _assimpScene = asDestScene;
        }

        void CopyFramesToScene(AnimationFile animationFile)
        {


        }

        public void MakeSkeleton()
        {
            Skeleton = new MSkeleton();
            Skeleton.InitSkeletonFromAnimFile(_bindPoseFile);
            CreateSceneSkeletonNodes(Skeleton);
        }

        public void CreateSceneSkeletonNodes(MSkeleton Skeleton)
        {
            _assimpScene.RootNode = new Node("ROOT_NODE");
            var parentList = new Assimp.Node[Skeleton.Bones.Length + 1];
            parentList[0] = _assimpScene.RootNode;

            //AddOneBinePoseFrame(Skeleton);

            for (int boneIndex = 0; boneIndex < Skeleton.Bones.Length; boneIndex++)
            {
                parentList[Skeleton.Bones[boneIndex].Index + 1] = CreateNewNode(Skeleton, parentList, boneIndex);
                parentList[Skeleton.Bones[boneIndex].ParentIndex + 1].Children.Add(parentList[Skeleton.Bones[boneIndex].Index + 1]);
            }
        }

        public static Assimp.Node CreateNewNode(MSkeleton asSkeleton, Assimp.Node[] parentList, int boneIndex)
        {
            var tempNode = new Node(asSkeleton.Bones[boneIndex].Name, parentList[asSkeleton.Bones[boneIndex].ParentIndex + 1]);

            tempNode.Transform = ToAssimpMatrix4x4(asSkeleton.Bones[boneIndex].LocalTransform);

            return tempNode;
        }

        public static Assimp.Matrix4x4 ToAssimpMatrix4x4(Matrix srcMat)
        {
            var srcMatTransposed = Matrix.Transpose(srcMat);
            var destMat4x4 = new Matrix4x4();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    destMat4x4[row + 1, col + 1] = srcMatTransposed[row, col];
                }
            }

            return destMat4x4;
        }



        void CopyFramesToAssimpScene(AnimationFile animationFile)
        {
            InitAnimation(animationFile);
        }

        public void InitAnimation(AnimationFile animationFile)
        {
            // fill 1 frame             

            _assimpScene.Animations.Add(new Animation());
            _assimpScene.Animations[0].Name = "testClip";                        

            _assimpScene.Animations[0].NodeAnimationChannels.AddRange(new NodeAnimationChannel[Skeleton.Bones.Length]);

            var frameCount = animationFile.Header.AnimationTotalPlayTimeInSec * animationFile.Header.FrameRate;

            _assimpScene.Animations[0].DurationInTicks = 3000;
            _assimpScene.Animations[0].TicksPerSecond = 1000;

            // TODO: switch to using "Add" for frame?
            for (int boneIndex = 0; boneIndex < Skeleton.Bones.Length; boneIndex++)
            {
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex] = new NodeAnimationChannel();
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].NodeName = Skeleton.Bones[boneIndex].Name;
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].PositionKeys.AddRange(new Assimp.VectorKey[animationFile.AnimationParts[0].DynamicFrames.Count]);
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].RotationKeys.AddRange(new Assimp.QuaternionKey[animationFile.AnimationParts[0].DynamicFrames.Count]);
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].ScalingKeys.AddRange(new Assimp.VectorKey[animationFile.AnimationParts[0].DynamicFrames.Count]);
            }
        }

        public void AddFrames(AnimationFile animationFile)
        {
            InitAnimation(animationFile);

            var keyFrames = CreateKeyFramesFromAnimationPart(_bindPoseFile, animationFile.AnimationParts[0]);
            
            //int frameIndex = 0;
            for (int frameIndex = 0; frameIndex < keyFrames.Count; frameIndex++)
            {
                SetOneFrame(keyFrames, 1.0f / animationFile.Header.FrameRate, frameIndex);
            }            
        }

        private void SetOneFrame(List<KeyFrame> KeyFrames, float frameTime,  int frameIndex)
        {
            for (int boneIndex = 0; boneIndex < Skeleton.Bones.Length; boneIndex++)
            {
                float time = frameTime * (float)frameIndex;
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].NodeName = Skeleton.Bones[boneIndex].Name;
                var transValue = new Assimp.Vector3D()
                {
                    X = -(KeyFrames[frameIndex].Position[boneIndex].X * AssimpExporter.SCALE_FACTOR),
                    Y = KeyFrames[frameIndex].Position[boneIndex].Y * AssimpExporter.SCALE_FACTOR,
                    Z = KeyFrames[frameIndex].Position[boneIndex].Z * AssimpExporter.SCALE_FACTOR,
                };

                // TODO: use Add here instead?
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].PositionKeys[frameIndex] = new VectorKey(time, transValue);

                var quatValue = new Assimp.Quaternion()
                {
                    X = KeyFrames[frameIndex].Rotation[boneIndex].X,
                    Y = -(KeyFrames[frameIndex].Rotation[boneIndex].Y),
                    Z = -(KeyFrames[frameIndex].Rotation[boneIndex].Z),
                    W = KeyFrames[frameIndex].Rotation[boneIndex].W,
                };

                // TODO: use Add here instead?
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].RotationKeys[frameIndex] = new QuaternionKey(time, quatValue);

                var scaleValue = new Assimp.Vector3D()
                {
                    X = 1.0f,
                    Y = 1.0f,
                    Z = 1.0f,
                };

                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].ScalingKeys[frameIndex] = new Assimp.VectorKey(time, scaleValue);
            }
        }
        private void SetOneFrameBindPose(AnimationFile bindPose, float frameTime,  int frameIndex)
        {
            for (int boneIndex = 0; boneIndex < Skeleton.Bones.Length; boneIndex++)
            {
                float time = frameTime * (float)frameIndex;
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].NodeName = Skeleton.Bones[boneIndex].Name;
                var transValue = new Assimp.Vector3D()
                {
                    X = -(bindPose.AnimationParts[0].DynamicFrames[frameIndex].Transforms[boneIndex].X* AssimpExporter.SCALE_FACTOR),
                    Y = bindPose.AnimationParts[0].DynamicFrames[frameIndex].Transforms[boneIndex].Y * AssimpExporter.SCALE_FACTOR,
                    Z = bindPose.AnimationParts[0].DynamicFrames[frameIndex].Transforms[boneIndex].Z * AssimpExporter.SCALE_FACTOR,
                };

                // TODO: use Add here instead?
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].PositionKeys[frameIndex] = new VectorKey(time, transValue);

                var quatValue = new Assimp.Quaternion()
                {
                    X = bindPose.AnimationParts[0].DynamicFrames[frameIndex].Quaternion[boneIndex].X,
                    Y = -(bindPose.AnimationParts[0].DynamicFrames[frameIndex].Quaternion[boneIndex].Y),
                    Z = -(bindPose.AnimationParts[0].DynamicFrames[frameIndex].Quaternion[boneIndex].Z),
                    W = bindPose.AnimationParts[0].DynamicFrames[frameIndex].Quaternion[boneIndex].W,
                };

                // TODO: use Add here instead?
                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].RotationKeys[frameIndex] = new QuaternionKey(time, quatValue);

                var scaleValue = new Assimp.Vector3D()
                {
                    X = 1.0f,
                    Y = 1.0f,
                    Z = 1.0f,
                };

                _assimpScene.Animations[0].NodeAnimationChannels[boneIndex].ScalingKeys[frameIndex] = new Assimp.VectorKey(time, scaleValue);
            }
        }


        List<KeyFrame> CreateKeyFramesFromAnimationPart(AnimationFile bindPose, AnimationPart animationPart)
        {
            List<KeyFrame> newDynamicFrames = new List<KeyFrame>();

            var animationSkeletonBoneCount = animationPart.RotationMappings.Count;
            var frameCount = animationPart.DynamicFrames.Count;

            if (frameCount == 0 && animationPart.StaticFrame != null)
                frameCount = 1; // Poses

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var newKeyframe = new KeyFrame();

                for (int animationSkeletonBoneIndex = 0; animationSkeletonBoneIndex < animationSkeletonBoneCount; animationSkeletonBoneIndex++)
                {
                    // We can apply animations to a skeleton where the skeleton of the animation is different then the skeleton we are applying it to
                    // If that is the case we just discard the information.
                    bool isBoneIndexValid = animationSkeletonBoneIndex < bindPose.Bones.Count();
                    if (isBoneIndexValid)
                    {
                        var translationLookup = animationPart.TranslationMappings[animationSkeletonBoneIndex];
                        if (translationLookup.IsDynamic)
                            newKeyframe.Position.Add(animationPart.DynamicFrames[frameIndex].Transforms[translationLookup.Id].ToVector3());
                        else if (translationLookup.IsStatic)
                            newKeyframe.Position.Add(animationPart.StaticFrame.Transforms[translationLookup.Id].ToVector3());
                        else
                            newKeyframe.Position.Add(bindPose.AnimationParts[0].DynamicFrames[0].Transforms[animationSkeletonBoneIndex].ToVector3());

                        var rotationLookup = animationPart.RotationMappings[animationSkeletonBoneIndex];
                        if (rotationLookup.IsDynamic)
                            newKeyframe.Rotation.Add(animationPart.DynamicFrames[frameIndex].Quaternion[rotationLookup.Id].ToQuaternion());
                        else if (rotationLookup.IsStatic)
                            newKeyframe.Rotation.Add(animationPart.StaticFrame.Quaternion[rotationLookup.Id].ToQuaternion());
                        else
                            newKeyframe.Rotation.Add(bindPose.AnimationParts[0].DynamicFrames[0].Quaternion[animationSkeletonBoneIndex].ToQuaternion());

                        newKeyframe.Scale.Add(Vector3.One);
                    }
                }

                newDynamicFrames.Add(newKeyframe);
            }

            return newDynamicFrames;
        }

        public class KeyFrame
        {
            public List<Vector3> Position { get; set; } = new List<Vector3>();
            public List<Quaternion> Rotation { get; set; } = new List<Quaternion>();
            public List<Vector3> Scale { get; set; } = new List<Vector3>();
        }
    }

}
