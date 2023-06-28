#include "FBXWRapperCPP_DLL.h"
#include "MeshProcessing/MeshProcessing.h"




void FBXSCeneContainer::readUnitsAndGeometry()
{
	auto sysUnit = m_fbxScene->GetGlobalSettings().GetSystemUnit();

	auto factor = m_fbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorFrom(sysUnit);	
	double multi = m_fbxScene->GetGlobalSettings().GetSystemUnit().GetMultiplier();	

	//auto factor_to_inches = m_fbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::Inch);
	auto m_factor_to_meters = m_fbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::m);
	auto m_factor_to_centimeater = m_fbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::cm);

	auto DEBUG_BREAK_1 = 1; // TODO: REMOVE;
}


bool FBXSCeneContainer::LoadSceneFile(const std::string& path)
{
	_log_action("Initializing FBX SDK importer...");
	auto m_pSdkManager = fbxsdk::FbxManager::Create();
	if (!m_pSdkManager)
		return _log_action_error("Initializing FBX SDK importer...");

	_log_action_success("Iinitializing FBX SDK importer...");

	// create an IOSettings object
	fbxsdk::FbxIOSettings* ios = fbxsdk::FbxIOSettings::Create(m_pSdkManager, IOSROOT);

	// set some IOSettings options
	ios->SetBoolProp(IMP_FBX_PIVOT, true);
	ios->SetBoolProp(IMP_FBX_MATERIAL, true);
	//ios->SetBoolProp(IMP_FBX_TEXTURE, true);
	ios->SetBoolProp(IMP_FBX_SHAPE, true);
	ios->SetBoolProp(IMP_FBX_GOBO, true);
	ios->SetBoolProp(IMP_FBX_ANIMATION, true);
	ios->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);

	// create an empty scene
	m_fbxScene = fbxsdk::FbxScene::Create(m_pSdkManager, "");

	// Create an importer.
	auto poImporter = fbxsdk::FbxImporter::Create(m_pSdkManager, "");

	// Initialize the importer by providing a filename and the IOSettings to use
	_log_action("Loading File: \"" + path + "\"");
	auto fileInitResult = poImporter->Initialize(path.c_str(), -1, ios);
	if (!fileInitResult)
	{
		_log_action("Error Loading File : \"" + path + " \". Stopping!");
		return false;
	}

	int x, y, z;
	poImporter->GetFileVersion(x, y, z);
	_log_action("File uses FBX Version " + std::to_string(x) + "." + std::to_string(y) + "." + std::to_string(z));

	// -- copies the loaded file into the scene!!
	if (!poImporter->Import(m_fbxScene))
	{
		_log_action("Import step failed. Fatal Error");

		return false;
	}
	//readUnitsAndGeometry();

	fbxsdk::FbxGeometryConverter oGeometryConverter(m_pSdkManager);
	_log_action("Triangulating....");
	bool bTri = oGeometryConverter.Triangulate(m_fbxScene, true, true);
	_log_action_success("Deep converting scene to 'DirectX' coord system...");


	fbxsdk::FbxAxisSystem oAxis(fbxsdk::FbxAxisSystem::DirectX);
	oAxis.DeepConvertScene(m_fbxScene);


	// The file is imported; so get rid of the importer.

	poImporter->Destroy();

}

bool FBXSCeneContainer::CreateSceneFromDiskFile(const std::string& path)
{
	if (!LoadSceneFile(path))
		return _log_action_error("Scene Loading Failed!!");

	std::vector<fbxsdk::FbxMesh*> fbxMeshList;
	FbxMeshProcessor::FindMeshesInScene(m_fbxScene, fbxMeshList);

	m_meshes.clear();
	m_meshes.resize(fbxMeshList.size());
	for (size_t meshIndex = 0; meshIndex < fbxMeshList.size(); meshIndex++)
	{
	
		FbxMeshProcessor::MakeUnindexPackedMesh(m_fbxScene, fbxMeshList[meshIndex],  m_meshes[meshIndex]);
	}
	auto DEBUG_BREAK = 1; // TODO: REMOVE!
}