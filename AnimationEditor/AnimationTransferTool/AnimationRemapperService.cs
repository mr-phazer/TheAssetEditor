﻿using CommonControls.Editors.BoneMapping;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using View3D.Animation;
using View3D.Utility;

namespace AnimationEditor.AnimationTransferTool
{
    public class AnimationRemapperService
    {
        List<IndexRemapping> _remappingInformaton;
        IEnumerable<SkeletonBoneNode> _bones;
        AnimationSettings _settings;

        public AnimationRemapperService(AnimationSettings settings, List<IndexRemapping> mapping, IEnumerable<SkeletonBoneNode> bones)
        {
            _settings = settings;
            _remappingInformaton = mapping;
            _bones = bones;
        }

        public AnimationClip ReMapAnimation(GameSkeleton copyFromSkeleton, GameSkeleton copyToSkeleton, AnimationClip animationToCopy)
        {
            var newFrameCount = (int)(_settings.SpeedMult.Value * animationToCopy.DynamicFrames.Count);
            var newPlayTime = (float)_settings.SpeedMult.Value * animationToCopy.PlayTimeInSec;

            animationToCopy.RemoveOptimizations(copyFromSkeleton);
            var resampledAnimationToCopy = View3D.Animation.AnimationEditor.ReSample(copyFromSkeleton, animationToCopy, newFrameCount, newPlayTime);

            var newAnimation = CreateNewAnimation(copyToSkeleton, resampledAnimationToCopy);

            if (copyFromSkeleton.SkeletonName != copyToSkeleton.SkeletonName)
                MapAnimationWorld(copyFromSkeleton, copyToSkeleton, resampledAnimationToCopy, newAnimation);

            if (_settings.ApplyRelativeScale.Value)
                ApplyRelativeScale(copyFromSkeleton, copyToSkeleton, newAnimation);

            SnapBonesToWorld(copyFromSkeleton, copyToSkeleton, newAnimation, resampledAnimationToCopy);

            if (_settings.FreezeUnmapped.Value)
                FrezeUnmappedBone(copyToSkeleton, newAnimation);

            FrezeTaggedBones(copyToSkeleton, newAnimation);
            ApplyOffsets(copyToSkeleton, newAnimation);
            FixAttachmentPoints(copyFromSkeleton, copyToSkeleton, newAnimation, resampledAnimationToCopy);
            ScaleAnimation(newAnimation, copyToSkeleton);

            return newAnimation;
        }

        public void MapAnimationWorld(GameSkeleton copyFromSkeleton, GameSkeleton copyToSkeleton, AnimationClip animationToCopy, AnimationClip newAnimation)
        {
            var frameCount = animationToCopy.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var currentCopyToFrame = AnimationSampler.Sample(frameIndex, 0, copyToSkeleton, new List<AnimationClip>() { newAnimation });
                    var copyFromFrame = AnimationSampler.Sample(frameIndex, 0, copyFromSkeleton, new List<AnimationClip>() { animationToCopy });

                    var desiredBonePosWorld = currentCopyToFrame.GetSkeletonAnimatedWorld(copyToSkeleton, i);

                    var mappedIndex = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == i);
                    if (mappedIndex != null)
                    {
                        var targetBoneIndex = mappedIndex.NewValue;
                        desiredBonePosWorld = copyFromFrame.GetSkeletonAnimatedWorld(copyFromSkeleton, targetBoneIndex) * Matrix.CreateScale(1);
                    }

                    var fromParentBoneIndex = copyToSkeleton.GetParentBone(i);
                    if (fromParentBoneIndex != -1)
                    {
                        var parentWorld = currentCopyToFrame.GetSkeletonAnimatedWorld(copyToSkeleton, fromParentBoneIndex);

                        var bonePositionLocalSpace = desiredBonePosWorld * Matrix.Invert(parentWorld);
                        bonePositionLocalSpace.Decompose(out var _, out var boneRotation, out var bonePosition);

                        newAnimation.DynamicFrames[frameIndex].Rotation[i] = boneRotation;
                        newAnimation.DynamicFrames[frameIndex].Position[i] = bonePosition;
                    }
                    else
                    {
                        desiredBonePosWorld.Decompose(out var _, out var boneRotation, out var bonePosition);

                        newAnimation.DynamicFrames[frameIndex].Rotation[i] = boneRotation;
                        newAnimation.DynamicFrames[frameIndex].Position[i] = bonePosition;
                    }
                }
            }
        }

        void ApplyRelativeScale(GameSkeleton copyFromSkeleton, GameSkeleton copyToSkeleton, AnimationClip animationToScale)
        {
            var frameCount = animationToScale.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var boneSettings = BoneHelper.GetBoneFromId(_bones, i);
                    var mappedIndex = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == i);

                    if (mappedIndex != null)
                    {
                        var targetBoneIndex = mappedIndex.NewValue;
                        var copyFromParentIndex = copyFromSkeleton.GetParentBone(targetBoneIndex);
                        var copyToParentIndex = copyToSkeleton.GetParentBone(i);

                        if (copyToParentIndex != -1 && copyFromParentIndex != -1)
                        {
                            var toBone0 = copyToSkeleton.GetWorldTransform(i).Translation;
                            var toBone1 = copyToSkeleton.GetWorldTransform(copyToParentIndex).Translation;
                            var targetBoneLength = Vector3.Distance(toBone0, toBone1);

                            var fromBone0 = copyFromSkeleton.GetWorldTransform(targetBoneIndex).Translation;
                            var fromBone1 = copyFromSkeleton.GetWorldTransform(copyFromParentIndex).Translation;
                            var fromBoneLength = Vector3.Distance(fromBone0, fromBone1);

                            if (fromBoneLength == 0 || targetBoneLength == 0)
                            {
                                targetBoneLength = 1;
                                fromBoneLength = 1;
                            }

                            var relativeScale = targetBoneLength / fromBoneLength;
                            animationToScale.DynamicFrames[frameIndex].Position[i] = animationToScale.DynamicFrames[frameIndex].Position[i] * relativeScale;
                        }
                    }
                }
            }
        }

        void SnapBonesToWorld(GameSkeleton copyFromSkeleton, GameSkeleton copyToSkeleton, AnimationClip animationToScale, AnimationClip animationToCopy)
        {
            var frameCount = animationToScale.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var copyFromFrame = AnimationSampler.Sample(frameIndex, 0, copyFromSkeleton, new List<AnimationClip>() { animationToCopy });

                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var currentFrame = AnimationSampler.Sample(frameIndex, 0, copyToSkeleton, new List<AnimationClip>() { animationToScale });

                    var boneSettings = BoneHelper.GetBoneFromId(_bones, i);
                    if (boneSettings.ForceSnapToWorld.Value == false)
                        continue;

                    var mappedIndex = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == i);
                    if (mappedIndex == null)
                        continue;

                    var fromParentBoneIndex = copyToSkeleton.GetParentBone(i);
                    if (fromParentBoneIndex == -1)
                        continue;

                    var targetBoneIndex = mappedIndex.NewValue;
                    var desiredBonePosWorld = copyFromFrame.GetSkeletonAnimatedWorld(copyFromSkeleton, targetBoneIndex) * Matrix.CreateScale(1);

                    var parentWorld = currentFrame.GetSkeletonAnimatedWorld(copyToSkeleton, fromParentBoneIndex);

                    var bonePositionLocalSpace = desiredBonePosWorld * Matrix.Invert(parentWorld);
                    bonePositionLocalSpace.Decompose(out var _, out var boneRotation, out var bonePosition);

                    // Apply the values to the animation
                    animationToScale.DynamicFrames[frameIndex].Rotation[i] = boneRotation;
                    animationToScale.DynamicFrames[frameIndex].Position[i] = bonePosition;
                }
            }
        }

        void ApplyOffsets(GameSkeleton copyToSkeleton, AnimationClip animationToScale)
        {
            var frameCount = animationToScale.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var currentFrame = AnimationSampler.Sample(frameIndex, 0, copyToSkeleton, new List<AnimationClip>() { animationToScale });

                    var fromParentBoneIndex = copyToSkeleton.GetParentBone(i);
                    if (fromParentBoneIndex == -1)
                        continue;

                    var boneSettings = BoneHelper.GetBoneFromId(_bones, i);
                    var desiredBonePosWorld = MathUtil.CreateRotation(new Vector3((float)boneSettings.RotationOffset.X.Value, (float)boneSettings.RotationOffset.Y.Value, (float)boneSettings.RotationOffset.Z.Value)) *
                        currentFrame.GetSkeletonAnimatedWorld(copyToSkeleton, i) *
                        Matrix.CreateTranslation(new Vector3((float)boneSettings.TranslationOffset.X.Value, (float)boneSettings.TranslationOffset.Y.Value, (float)boneSettings.TranslationOffset.Z.Value));

                    var parentWorld = currentFrame.GetSkeletonAnimatedWorld(copyToSkeleton, fromParentBoneIndex);
                    var bonePositionLocalSpace = desiredBonePosWorld * Matrix.Invert(parentWorld);
                    bonePositionLocalSpace.Decompose(out var _, out var boneRotation, out var bonePosition);

                    animationToScale.DynamicFrames[frameIndex].Rotation[i] = boneRotation;
                    animationToScale.DynamicFrames[frameIndex].Position[i] = bonePosition;

                    if (boneSettings.IsLocalOffset.Value)
                    {
                        // Todo - Some inverse fuckery to children
                        var childBones = copyToSkeleton.GetDirectChildBones(i);
                    }
                }
            }
        }

        void FixAttachmentPoints(GameSkeleton copyFromSkeleton, GameSkeleton copyToSkeleton, AnimationClip animationToFix, AnimationClip animationToCopy)
        {
            var frameCount = animationToCopy.DynamicFrames.Count;

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    // Does this bone have a thing to fix?
                    var boneSettings = BoneHelper.GetBoneFromId(_bones, i);

                    var mappedIndex = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == i);
                    if (boneSettings.SelectedRelativeBone == null || mappedIndex == null)
                        continue;

                    var targetBoneIndex = mappedIndex.NewValue;

                    var currentCopyToFrame = AnimationSampler.Sample(frameIndex, 0, copyToSkeleton, new List<AnimationClip>() { animationToFix });
                    var copyFromFrame = AnimationSampler.Sample(frameIndex, 0, copyFromSkeleton, new List<AnimationClip>() { animationToCopy });


                    // self attach - The attachment point to move | copyToSkeleton -> boneIndex i
                    // target attach - the attachment point to move to |  copyFromSkeleton -> boneIndex targetBoneIndex
                    // self hand - Reference point | copyToSkeleton -> boneIndex SelectedRelativeBone.index
                    // target hand-  reference point | copyFromSkeleton -> boneIndex self hand mapping index

                    var boneIndexAttachmentPointSelf = i;
                    var boneIndexHandSelf = boneSettings.SelectedRelativeBone.BoneIndex.Value;


                    var boneIndexAttachmentPointSource = mappedIndex.NewValue;
                    var mappedIndexRef = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == boneIndexHandSelf);
                    var boneIndexHandSource = mappedIndexRef.NewValue;


                    var self = copyFromFrame.GetSkeletonAnimatedWorld(copyFromSkeleton, boneIndexAttachmentPointSource);
                    var hand = copyFromFrame.GetSkeletonAnimatedWorld(copyFromSkeleton, boneIndexHandSource);

                    self.Decompose(out var _, out var _, out var bone0);
                    hand.Decompose(out var _, out var _, out var bone1);

                    var diff = bone0 - bone1;

                    var desiredBonePosWorld = currentCopyToFrame.GetSkeletonAnimatedWorld(copyToSkeleton, boneIndexHandSelf);

                    desiredBonePosWorld = /*MathUtil.CreateRotation(new Vector3((float)boneSettings.RotationOffset.X.Value, (float)boneSettings.RotationOffset.Y.Value, (float)boneSettings.RotationOffset.Z.Value)) **/
                      desiredBonePosWorld *
                       Matrix.CreateTranslation(diff);

                    // Reapply offsets
                    desiredBonePosWorld = MathUtil.CreateRotation(new Vector3((float)boneSettings.RotationOffset.X.Value, (float)boneSettings.RotationOffset.Y.Value, (float)boneSettings.RotationOffset.Z.Value)) *
                        desiredBonePosWorld *
                        Matrix.CreateTranslation(new Vector3((float)boneSettings.TranslationOffset.X.Value, (float)boneSettings.TranslationOffset.Y.Value, (float)boneSettings.TranslationOffset.Z.Value));

                    //   desiredBonePosWorld = copyFromFrame.GetSkeletonAnimatedWorld(copyFromSkeleton, targetBoneIndex) * Matrix.CreateScale(1);


                    var fromParentBoneIndex = copyToSkeleton.GetParentBone(i);

                    var parentWorld = currentCopyToFrame.GetSkeletonAnimatedWorld(copyToSkeleton, fromParentBoneIndex);

                    var bonePositionLocalSpace = desiredBonePosWorld * Matrix.Invert(parentWorld);
                    bonePositionLocalSpace.Decompose(out var _, out var boneRotation, out var bonePosition);

                    //animationToFix.DynamicFrames[frameIndex].Rotation[i] = boneRotation;
                    animationToFix.DynamicFrames[frameIndex].Position[i] = bonePosition;

                }
            }
        }

        void ScaleAnimation(AnimationClip animation, GameSkeleton skeleton)
        {
            var frameCount = animation.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < skeleton.BoneCount; i++)
                {
                    animation.DynamicFrames[frameIndex].Position[i] = animation.DynamicFrames[frameIndex].Position[i] * (float)_settings.Scale.Value;
                }
            }

        }

        void FrezeUnmappedBone(GameSkeleton copyToSkeleton, AnimationClip animation)
        {
            var frameCount = animation.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var mappedIndex = _remappingInformaton.FirstOrDefault(x => x.OriginalValue == i);
                    if (mappedIndex != null)
                        continue;

                    animation.DynamicFrames[frameIndex].Rotation[i] = Quaternion.Identity;
                    animation.DynamicFrames[frameIndex].Position[i] = Vector3.Zero;
                }
            }
        }


        void FrezeTaggedBones(GameSkeleton copyToSkeleton, AnimationClip animation)
        {
            var frameCount = animation.DynamicFrames.Count;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                for (int i = 0; i < copyToSkeleton.BoneCount; i++)
                {
                    var boneSettings = BoneHelper.GetBoneFromId(_bones, i);
                    if (boneSettings.FreezeTranslation.Value)
                        animation.DynamicFrames[frameIndex].Position[i] = Vector3.Zero;
                }
            }
        }

        AnimationClip CreateNewAnimation(GameSkeleton skeleton, AnimationClip animationToCopy)
        {
            var frameCount = animationToCopy.DynamicFrames.Count;

            var newAnimation = new AnimationClip();
            newAnimation.PlayTimeInSec = animationToCopy.PlayTimeInSec;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                newAnimation.DynamicFrames.Add(new AnimationClip.KeyFrame());
                for (int i = 0; i < skeleton.BoneCount; i++)
                {
                    newAnimation.DynamicFrames[frameIndex].Rotation.Add(skeleton.Rotation[i]);
                    newAnimation.DynamicFrames[frameIndex].Position.Add(skeleton.Translation[i]);
                    newAnimation.DynamicFrames[frameIndex].Scale.Add(Vector3.One);
                }
            }

            for (int i = 0; i < skeleton.BoneCount; i++)
            {
                newAnimation.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                newAnimation.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                newAnimation.DynamicFrames[0].Scale[0] = Vector3.One;
            }
            return newAnimation;
        }
    }

    public static class BoneHelper
    {
        public static SkeletonBoneNode GetBoneFromId(IEnumerable<SkeletonBoneNode> root, int boneId)
        {
            foreach (SkeletonBoneNode item in root)
            {
                if (item.BoneIndex.Value == boneId)
                    return item;

                var result = GetBoneFromId(item.Children, boneId);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}