#include "FBXSceneLoaderService.h"

#include "..\Helpers\Geometry\FBXNodeSearcher.h"
#include "..\Processing\FBXMeshProcessor.h"
#include "..\Processing\FBXSkinProcessor.h"
#include "..\Processing\FBXMeshCreator.h"


bool wrapdll::FBXImporterService::LoadFile(const std::string& path)
{
	m_poFbxScene = FBXHelperFileUtil::CreateScene(path);
	if (!m_poFbxScene)
		return log_action_error("Scene Loading Failed!!");

	return true;
}

wrapdll::FBXSCeneContainer* wrapdll::FBXImporterService::ProcessAndFillScene()
{
	auto& destPackedMeshes = m_sceneContainer.GetMeshes();

	std::vector<fbxsdk::FbxMesh*> fbxMeshList;
	FBXNodeSearcher::FindMeshesInScene(m_poFbxScene, fbxMeshList);

	GetBonesFromsFbxScene();

	destPackedMeshes.clear();
	destPackedMeshes.resize(fbxMeshList.size());
	for (size_t meshIndex = 0; meshIndex < fbxMeshList.size(); meshIndex++)
	{
		std::vector<ControlPointInfluences> vertexToControlPoint;
		FBXSkinProcessorService::ProcessSkin(fbxMeshList[meshIndex], destPackedMeshes[meshIndex], m_animFileBoneNames, vertexToControlPoint);
		FBXMeshCreator::MakeUnindexPackedMesh(m_poFbxScene, fbxMeshList[meshIndex], destPackedMeshes[meshIndex], vertexToControlPoint);

		log_action("Doing Tangents/Indexing");
		FbxMeshProcessor::doTangentAndIndexing(destPackedMeshes[meshIndex]);
		log_action("Done Tangents/Indexing");
	}
	auto DEBUG_BREAK = 1; // TODO: REMOVE!

	return &m_sceneContainer;
}

extern "C"
{
	FBXWRAPPERDLL_API_EXT
	wrapdll::FBXImporterService* CreateSceneFBX(char* path)
	{
		auto pInstance = new wrapdll::FBXImporterService();
		pInstance->LoadFile(path);
		return pInstance;
	};

	FBXWRAPPERDLL_API_EXT
		wrapdll::FBXSCeneContainer* ProcessAndFillScene(wrapdll::FBXImporterService* pInstance)
	{
		pInstance->ProcessAndFillScene();

		return &pInstance->GetSceneContainer();
	};

	FBXWRAPPERDLL_API_EXT
		char* GetSkeletonNameFromScene(wrapdll::FBXImporterService* pInstance, int meshindex)
	{
		return (char*)pInstance->GetSkeletonNameFromSceneNodes();
	};

	FBXWRAPPERDLL_API_EXT
		void AddBoneName(wrapdll::FBXImporterService* pInstance, char* boneName, int len)
	{
		std::string tempBoneName(boneName, len);
		pInstance->GetBoneNames().push_back(tempBoneName);
	};

	FBXWRAPPERDLL_API_EXT
		void ClearBoneNames(wrapdll::FBXImporterService* pInstance)
	{
		pInstance->GetBoneNames().clear();
	};

	FBXWRAPPERDLL_API_EXT
	void DeleteBaseObj(BaseObject* pInstance)
	{
		if (pInstance != nullptr)
		{
			delete pInstance;
			pInstance = nullptr;
		}
	};
}