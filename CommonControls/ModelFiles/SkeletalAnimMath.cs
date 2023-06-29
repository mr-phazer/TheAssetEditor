using CommonControls.FileTypes.Animation;
using Microsoft.Xna.Framework;
using static CommonControls.Editors.AnimationPack.Converters.AnimationBinFileToXmlConverter;

namespace CommonControls.ModelFiles
{
    public class BoneNode
    {
        public string Name { get; set; } = "";
        public int Index { get; set; }
        public int ParentIndex { get; set; }
        public Matrix LocalTransform { get; set; } = Matrix.Identity;
        public Matrix GlobalTransform { get; set; } = Matrix.Identity;
        public Vector3 LocalTranslation { get; set; } = Vector3.Zero;
        public Quaternion LocalRotation { get; set; } = Quaternion.Identity;
        public Vector3 GlobalTranslation { get; set; } = Vector3.Zero;
        public Quaternion GlobalRotation { get; set; } = Quaternion.Identity;

        public BoneNode()
        {
        }

        public BoneNode(string name, int parentIndex, Vector3 localTranslation, Quaternion localRotation)
        {
            Name = name;
            ParentIndex = parentIndex;
            LocalTranslation = localTranslation;
            LocalRotation = localRotation;
        }
    }

    public class MSkeleton
    {
        public BoneNode[] Bones { get; private set; }



        public Matrix[] InverseBindpose { set; get; }

        public MSkeleton()
        {
        }

        public void InitSkeletonFromAnimFile(AnimationFile bindPoseAnimFile)
        {
            FillSkeletonFromAnimFile(bindPoseAnimFile);
            CalculateGlobalTransforms();
        }

        private void FillSkeletonFromAnimFile(AnimationFile bindPoseAnimFile)
        {
            Bones = new BoneNode[bindPoseAnimFile.Bones.Length];
            InverseBindpose = new Matrix[bindPoseAnimFile.Bones.Length];

            var animClipFragMent = 0;
            var frameIndex = 0;
            for (int boneIndex = 0; boneIndex < Bones.Length; boneIndex++)
            {
                Bones[boneIndex] = new BoneNode();
                Bones[boneIndex].Name = bindPoseAnimFile.Bones[boneIndex].Name;
                Bones[boneIndex].Index = bindPoseAnimFile.Bones[boneIndex].Id;
                Bones[boneIndex].ParentIndex = bindPoseAnimFile.Bones[boneIndex].ParentId;

                // The minuses are a weird hack a found, on a math forum, to invert a skeleton's handedness (left <-> right)
                // It works perfectly, tho I don't fully understand it
                Bones[boneIndex].LocalTranslation = new Vector3()
                {
                    X = -(bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Transforms[boneIndex].X),
                    Y = bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Transforms[boneIndex].Y,
                    Z = bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Transforms[boneIndex].Z,
                };

                Bones[boneIndex].LocalRotation = new Quaternion()
                {
                    X = bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Quaternion[boneIndex].X,
                    Y = -(bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Quaternion[boneIndex].Y),
                    Z = -(bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Quaternion[boneIndex].Z),
                    W = bindPoseAnimFile.AnimationParts[animClipFragMent].DynamicFrames[frameIndex].Quaternion[boneIndex].W,
                };

                var translationMatrix = Matrix.CreateTranslation(Bones[boneIndex].LocalTranslation * AssimpExporter.SCALE_FACTOR);
                var rotationMatrix = Matrix.CreateFromQuaternion(Bones[boneIndex].LocalRotation);
                Bones[boneIndex].LocalTransform = rotationMatrix * translationMatrix;
            }
        }

        private void CalculateGlobalTransforms()
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                ref var bone = ref Bones[i];
                if (bone.ParentIndex != -1)
                {
                    ref var parent = ref Bones[Bones[i].ParentIndex];

                    bone.GlobalRotation = bone.LocalRotation * parent.GlobalRotation;

                    bone.GlobalTranslation =
                        parent.GlobalTranslation +
                        Vector3.Transform(bone.LocalTranslation, parent.GlobalRotation);
                }
                else
                {
                    Bones[i].GlobalTransform = Bones[i].LocalTransform;
                }

                var globalTranslationMatrix = Matrix.CreateTranslation(bone.GlobalTranslation);
                var gobalRotationMatrix = Matrix.CreateFromQuaternion(bone.GlobalRotation);
                InverseBindpose[i] = Matrix.Invert(gobalRotationMatrix * globalTranslationMatrix);
            }
        }

        public void TransFormBoneLocalRotation(int boneIndex, Matrix transform)
        {
            var matrixFromQuat = Matrix.CreateFromQuaternion(Bones[boneIndex].LocalRotation);
            matrixFromQuat *= transform;
            Bones[boneIndex].LocalRotation = Quaternion.CreateFromRotationMatrix(matrixFromQuat);

            // make the local transform
            var translationMatrix = Matrix.CreateTranslation(Bones[boneIndex].LocalTranslation * AssimpExporter.SCALE_FACTOR);
            var rotationMatrix = Matrix.CreateFromQuaternion(Bones[boneIndex].LocalRotation);
            InverseBindpose[boneIndex] = Matrix.Invert(rotationMatrix * translationMatrix);
        }

        void TransFormBoneLocalTranslation(int boneIndex, Matrix transform)
        {
            Bones[boneIndex].LocalTranslation = Vector3.Transform(Bones[boneIndex].LocalTranslation, transform);
        }
    }
    public class Helpers
    {
        Quaternion RotateQuaternionByMatrix(Quaternion quaternion, Matrix transform)
        {
            transform.Decompose(out var scale, out var quaternionFromMatrix, out var translation); // get rotational part of matrix
            return quaternion * quaternionFromMatrix;
        }


    }
}
