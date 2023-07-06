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


