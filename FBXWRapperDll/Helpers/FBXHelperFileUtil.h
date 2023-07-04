#pragma once

#include <fbxsdk.h>
#include "..\Logging\Logging.h"

namespace wrapdll
{
	class FBXHelperFileUtil
	{
	public:
		static fbxsdk::FbxScene* CreateScene(const std::string path)
		{
			_log_action("Initializing FBX SDK importer...");
			auto m_pSdkManager = fbxsdk::FbxManager::Create();
			if (!m_pSdkManager)
			{
				_log_action_error("Initializing FBX SDK importer...");
				return nullptr;
			}

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
			auto fbxScene = fbxsdk::FbxScene::Create(m_pSdkManager, "");

			// Create an importer.
			auto poImporter = fbxsdk::FbxImporter::Create(m_pSdkManager, "");

			// Initialize the importer by providing a filename and the IOSettings to use
			_log_action("Loading File: \"" + path + "\"");
			auto fileInitResult = poImporter->Initialize(path.c_str(), -1, ios);
			if (!fileInitResult)
			{
				_log_action("Error Loading File : \"" + path + " \". Stopping!");
				return nullptr;
			}

			int x, y, z;
			poImporter->GetFileVersion(x, y, z);
			_log_action("File uses FBX Version " + std::to_string(x) + "." + std::to_string(y) + "." + std::to_string(z));

			// -- copies the loaded file into the scene!!
			if (!poImporter->Import(fbxScene))
			{
				_log_action_error("Import step failed. Fatal Error");

				return nullptr;
			}
			//readUnitsAndGeometry();

			fbxsdk::FbxGeometryConverter oGeometryConverter(m_pSdkManager);
			_log_action("Triangulating....");
			bool bTri = oGeometryConverter.Triangulate(fbxScene, true, true);
			_log_action_success("Deep converting scene to 'DirectX' coord system...");


			fbxsdk::FbxAxisSystem oAxis(fbxsdk::FbxAxisSystem::DirectX);
			oAxis.DeepConvertScene(fbxScene);


			// The file is imported; so get rid of the importer.

			poImporter->Destroy();

			return fbxScene;
		}
	};
}