#pragma once

#include <algorithm>
#include <vector>
#include <string>

#include "FBXNodeSearcher.h"

namespace wrapdll
{
	struct BoneInfo
	{
		std::string name = "{NOT SET}";
		int id = -1;
		int parentId = -1;
		int tempIndex = -1;
	};

	static bool comparefn(BoneInfo& a, BoneInfo& b)
	{
		return a.parentId < b.parentId;
	}

	static void SwapBones(BoneInfo& a, BoneInfo& b)
	{
		BoneInfo temBoneInfo = a;
		a = b;
		b = temBoneInfo;
	}


	static void ChangeParentIndexes(std::vector<BoneInfo>& bones, int oldIndex, int newIndex)
	{
		for (auto& bone : bones)
		{
			if (bone.parentId == oldIndex)
			{
				bone.tempIndex = newIndex;
			}
		}
	}

	static void UpdateIndexes(std::vector<BoneInfo>& bones)
	{
		for (auto& bone : bones)
		{
			bone.parentId == bone.tempIndex;							
		}
	}

	static void SortBubbleAndUpdateIndexes(std::vector<BoneInfo>& bones)
	{
		bool bDone = true;
		do
		{
			bDone = true;
			for (size_t i = 0; i < bones.size(); i++)
			{
				if (i + 1 < bones.size()) // don't go out bounds
				{
					if (bones[i].parentId > bones[i + 1].parentId)
					{
						bDone = false;			
												
						SwapBones(bones[i], bones[i + 1]);
						bones[i].id = i;
						bones[i + 1].id = i+1;											

						ChangeParentIndexes(bones, i, i+1);
						ChangeParentIndexes(bones, i+1, i);

						UpdateIndexes(bones);
						
					}
				}
			}
		} while (!bDone);
	}

	static void SortBubbleAlphaAndUpdateIndexes(std::vector<BoneInfo>& bones)
	{
		bool bDone = true;
		do
		{
			bDone = true;
			for (size_t i = 0; i < bones.size(); i++)
			{


				if (i + 1 < bones.size()) // don't go out bounds
				{


					if (bones[i].name > bones[i + 1].name &&  bones[i].parentId <= bones[i + 1].parentId)					
					{
						bDone = false;			
												
						SwapBones(bones[i], bones[i + 1]);
						bones[i].id = i;
						bones[i + 1].id = i+1;											

						ChangeParentIndexes(bones, i, i+1);
						ChangeParentIndexes(bones, i+1, i);

						UpdateIndexes(bones);
						
					}
				}
			}
		} while (!bDone);
	}



	class FBXBoneSorter
	{
		// TODO: MOVE / REMOVE, don't include in PR if it doesn't work 
	public:
		static std::vector<BoneInfo> GetBonesFromsFbxScene(fbxsdk::FbxScene* m_poFbxScene)
		{
			std::vector<fbxsdk::FbxNode*> fbxNodes;
			FBXNodeSearcher::FindFbxNodesByType(fbxsdk::FbxNodeAttribute::EType::eSkeleton, m_poFbxScene, fbxNodes);


			auto boneInfoList = FromBoneNodeToBoneInfo(fbxNodes);;

			SortBubbleAndUpdateIndexes(boneInfoList);
			SortBubbleAlphaAndUpdateIndexes(boneInfoList);

			return boneInfoList;

			/*
				algo do bubble sort loop
				if (bones[i].parentId > bone[i+1].parentId)
				{
					swap(bones[i], bone[i+1])

					swap(a, b)
					{
						swap changes id of both a and b

						do for both a and b
						 update_all_parents_id(old_parent_id, new_parent_id)
					}
				}

				maybe add conditional sort, where alphabetical order is prefered unless it makes change descending parant ids

			*/



			auto DEBUG_BREAK = 1; // TODO: REMOVE!
		};

		/// <summary>
		/// Fills BoneInfo list, sets correct parent indexes
		/// </summary>		
		static std::vector<BoneInfo> FromBoneNodeToBoneInfo(const std::vector<fbxsdk::FbxNode*>& fbxNodes)
		{
			std::vector<BoneInfo> bones;
			for (size_t boneNodeIndex = 0; boneNodeIndex < fbxNodes.size(); boneNodeIndex++)
			{
				std::string boneName = fbxNodes[boneNodeIndex]->GetName();

				if (boneName.find("skeleton//") == 0)
					continue;

				BoneInfo newBone = { boneName , boneNodeIndex, -1 };

				for (size_t boneParentIndex = 0; boneParentIndex < fbxNodes.size(); boneParentIndex++)
				{
					// find parent by comparing pointers
					if (fbxNodes[boneNodeIndex]->GetParent() == fbxNodes[boneParentIndex])
					{
						newBone.parentId = boneParentIndex;
					}
				}

				bones.push_back(newBone);
			}

			return bones;
		}

	};
}
