#pragma once

#include <algorithm>
#include "..\FBXScene\FBXSCeneContainer.h"

namespace wrapdll
{
	class FBXImporterService : public BaseObject
	{
	public:
		FBXImporterService() {};

		FBXImporterService* Create(const std::string& path);
		
		virtual ~FBXImporterService()
		{			
			auto DEBUG_BREAK = 1; // TODO: remove 
		};

		/// <summary>
		/// Makes and fills a FBXSceneContainer
		/// </summary>		
		bool LoadFile(const std::string& path);
		FBXSCeneContainer* ProcessAndFillScene();

		void FillBoneNames(const std::vector<std::string>& boneNames)
		{
			m_animFileBoneNames = boneNames;
		}

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

		void GetBonesFromsFbxScene()
		{
			std::vector<fbxsdk::FbxNode*> fbxNodes;
			FBXNodeSearcher::FindFbxNodesByType(fbxsdk::FbxNodeAttribute::EType::eSkeleton, m_poFbxScene, fbxNodes);

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

				maybe add conditional sort, where alphabetical order is prefered unless it make change descending parant ids
			
			*/



			auto DEBUG_BREAK = 1; // TODO: REMOVE!
		}

		const char* GetSkeletonNameFromSceneNodes()
		{
			// TODO: should copy this to SceneContainer
			m_skeletonName = FBXNodeSearcher::FetchSkeletonNameFromScene(m_poFbxScene);
			return m_skeletonName.c_str();
		}

		std::vector<std::string>& GetBoneNames()
		{
			return m_animFileBoneNames;
		}

		FBXSCeneContainer& GetSceneContainer()
		{
			return m_sceneContainer;
		}

	private:
		std::string m_skeletonName = "";
		std::vector<std::string> m_animFileBoneNames; // ordered as the .ANIM file, so can be used for bonename -> index lookups
		FBXSCeneContainer m_sceneContainer;
		fbxsdk::FbxScene* m_poFbxScene = nullptr;
	};

};


