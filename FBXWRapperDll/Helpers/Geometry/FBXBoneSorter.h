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
	};

	static bool comparefn(BoneInfo& a, BoneInfo& b)
	{
		a.name = "sorted";
		a.name = "alsosorted";
		return a.parentId < b.parentId;

	}

	void swap(BoneInfo& a, BoneInfo& b)
	{


	}



	class FBXBoneSorter
	{
		// TODO: MOVE / REMOVE, don't include in PR if it doesn't work 
		void GetBonesFromsFbxScene(fbxsdk::FbxScene* m_poFbxScene)
		{
			std::vector<fbxsdk::FbxNode*> fbxNodes;
			FBXNodeSearcher::FindFbxNodesByType(fbxsdk::FbxNodeAttribute::EType::eSkeleton, m_poFbxScene, fbxNodes);


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
		std::vector<BoneInfo> FromBoneNodeToBoneInfo(const std::vector<fbxsdk::FbxNode*>& fbxNodes)
		{
			std::vector<BoneInfo> bones;
			for (size_t boneNodeIndex = 0; boneNodeIndex < fbxNodes.size(); boneNodeIndex++)
			{
				BoneInfo newBone = { fbxNodes[boneNodeIndex]->GetName(), boneNodeIndex, -1 };

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

		}

	};
}
