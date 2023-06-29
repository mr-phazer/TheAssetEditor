#pragma once

#pragma once
#include <fbxsdk.h>
#include <vector>
#include <map>
#include "..\Logging\Logging.h"

class MeshProcessingHelper
{
public:
	static double GetFactorToMeters(fbxsdk::FbxScene* poFbxScene)
	{
		if (!poFbxScene)
		{
			_log_action_error("Passed FbxScene == nullptr. Returning stardard factor");
			return 1.0;
		}

		return poFbxScene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(fbxsdk::FbxSystemUnit::m);
	}
};

