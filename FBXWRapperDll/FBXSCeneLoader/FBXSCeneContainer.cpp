#include "FBXSceneContainer.h"
#include "..\FBXServices/FBXMeshProcessor.h"
#include "..\FBXServices/FBXSkinService.h"
#include "..\Helpers\FBXHelperFileUtil.h"
#include "..\Helpers\Gemotryhelp\FBXNodeSearcher.h"
#include "..\FBXServices\FBXMeshProcessor.h"


using namespace wrapdll;

void FBXSCeneContainer::readUnitsAndGeometry()
{
	auto sysUnit = m_poFbxScene->GetGlobalSettings().GetSystemUnit();

	auto factor = m_poFbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorFrom(sysUnit);
	double multi = m_poFbxScene->GetGlobalSettings().GetSystemUnit().GetMultiplier();

	//auto factor_to_inches = m_poFbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::Inch);
	auto m_factor_to_meters = m_poFbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::m);
	auto m_factor_to_centimeater = m_poFbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::cm);

	auto DEBUG_BREAK_1 = 1; // TODO: REMOVE;
}


//bool FBXSCeneContainer::LoadSceneFromFBXFile(const std::string& path)
//{
//	//_log_action("Initializing FBX SDK importer...");
//	//auto m_pSdkManager = fbxsdk::FbxManager::Create();
//	//if (!m_pSdkManager)
//	//	return _log_action_error("Initializing FBX SDK importer...");
//
//	//_log_action_success("Iinitializing FBX SDK importer...");
//
//	//// create an IOSettings object
//	//fbxsdk::FbxIOSettings* ios = fbxsdk::FbxIOSettings::Create(m_pSdkManager, IOSROOT);
//
//	//// set some IOSettings options
//	//ios->SetBoolProp(IMP_FBX_PIVOT, true);
//	//ios->SetBoolProp(IMP_FBX_MATERIAL, true);
//	////ios->SetBoolProp(IMP_FBX_TEXTURE, true);
//	//ios->SetBoolProp(IMP_FBX_SHAPE, true);
//	//ios->SetBoolProp(IMP_FBX_GOBO, true);
//	//ios->SetBoolProp(IMP_FBX_ANIMATION, true);
//	//ios->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
//
//	//// create an empty scene
//	//m_poFbx = fbxsdk::FbxScene::Create(m_pSdkManager, "");
//
//	//// Create an importer.
//	//auto poImporter = fbxsdk::FbxImporter::Create(m_pSdkManager, "");
//
//	//// Initialize the importer by providing a filename and the IOSettings to use
//	//_log_action("Loading File: \"" + path + "\"");
//	//auto fileInitResult = poImporter->Initialize(path.c_str(), -1, ios);
//	//if (!fileInitResult)
//	//{
//	//	_log_action("Error Loading File : \"" + path + " \". Stopping!");
//	//	return false;
//	//}
//
//	//int x, y, z;
//	//poImporter->GetFileVersion(x, y, z);
//	//_log_action("File uses FBX Version " + std::to_string(x) + "." + std::to_string(y) + "." + std::to_string(z));
//
//	//// -- copies the loaded file into the scene!!
//	//if (!poImporter->Import(m_poFbxScene))
//	//{
//	//	_log_action("Import step failed. Fatal Error");
//
//	//	return false;
//	//}
//	////readUnitsAndGeometry();
//
//	//fbxsdk::FbxGeometryConverter oGeometryConverter(m_pSdkManager);
//	//_log_action("Triangulating....");
//	//bool bTri = oGeometryConverter.Triangulate(m_poFbxScene, true, true);
//	//_log_action_success("Deep converting scene to 'DirectX' coord system...");
//
//
//	//fbxsdk::FbxAxisSystem oAxis(fbxsdk::FbxAxisSystem::DirectX);
//	//oAxis.DeepConvertScene(m_poFbxScene);
//
//
//	//// The file is imported; so get rid of the importer.
//
//	//poImporter->Destroy();
//
//	return true;
//}

bool FBXSCeneContainer::CreateSceneFromDiskFile(const std::string& path)
{
	m_poFbxScene = FBXHelperFileUtil::CreateScene(path);
	if (!m_poFbxScene)
		return _log_action_error("Scene Loading Failed!!");

	std::vector<fbxsdk::FbxMesh*> fbxMeshList;
	FBXNodeSearcher::FindMeshesInScene(m_poFbxScene, fbxMeshList);

	m_packedMeshes.clear();
	m_packedMeshes.resize(fbxMeshList.size());
	for (size_t meshIndex = 0; meshIndex < fbxMeshList.size(); meshIndex++)
	{
		std::vector<ControlPointInfluences> vertexToControlPoint;
		FbxMeshProcessor::MakeUnindexPackedMesh(m_poFbxScene, fbxMeshList[meshIndex], m_packedMeshes[meshIndex], vertexToControlPoint);
		FBXSkinProcessorService::ProcessSkin(fbxMeshList[meshIndex], m_packedMeshes[meshIndex], m_animFileBoneNames, vertexToControlPoint);
		//FbxMeshProcessor::doTangentAndIndexing(m_packedMeshes[meshIndex]);
	}
	auto DEBUG_BREAK = 1; // TODO: REMOVE!

	return true;
}