#include "FBXSkinService.h"

bool FBXSkinProcessorService::ProcessSkin(FbxMesh* _poSourceFbxMesh, PackedMesh& destPackedMesh, const std::vector <std::string>& boneTable, const std::vector<int>& vertexToControlPoint)
{
	//int controlPointCount = _poSourceFbxMesh->GetControlPointsCount();
	_log_action("Processing Weighing for mesh: "+ std::string(_poSourceFbxMesh->GetName()));
	//// reset for new skin, assume skin 0 is the rigging for all meshes in file
	//m_vecvecTempControlPointInfluences.clear();
	//m_vecvecTempControlPointInfluences.resize(ctrl_point_count);

	int deformerCount = _poSourceFbxMesh->GetDeformerCount();

	// Error checking,
	if (deformerCount < 1)
	{
		return _log_action_warning(std::string(_poSourceFbxMesh->GetName()) + ": no deformer/skin modifier found. No Rigging Will Be Added.");
	}

	// get skin 0, if there are more than one skin modifier, it is the users resposibility to make sure that skin 0 is the  "mesh skin"
	fbxsdk::FbxSkin* pSkin = (FbxSkin*)_poSourceFbxMesh->GetDeformer(0);
	if (!pSkin)
	{
		return _log_action_warning(std::string(_poSourceFbxMesh->GetName()) + ":pSkin == NULL ");
	}

	return ExtractInfluencesFromSkin(pSkin, _poSourceFbxMesh,  destPackedMesh, boneTable, vertexToControlPoint);
}

bool FBXSkinProcessorService::ExtractInfluencesFromSkin(fbxsdk::FbxSkin* poSkin, fbxsdk::FbxMesh* poFbxMeshNode, PackedMesh& destPackedMesh, const std::vector<std::string>& boneTable, const std::vector<int>& vertexToControlPoint)
{
	_log_action("Skin Name: " + std::string(poSkin->GetName()));

	int cluster_count = poSkin->GetClusterCount();

	// check if there is no rigging data for this skin
	if (cluster_count < 1)
	{
		return _log_action_warning(std::string(poFbxMeshNode->GetName()) + ": no weighting data found for skin: " + std::string(poSkin->GetName()) + " !");
	}

	// Rund through all clusters (1 cluster = 1 bone)
	for (int clusterIndex = 0; clusterIndex < cluster_count; clusterIndex++)
	{
		// Get the collection of clusters {vertex, weight}
		fbxsdk::FbxCluster* pCluster = poSkin->GetCluster(clusterIndex);

		// Get the "bone = the node which is affecting this FbxCluster
		fbxsdk::FbxNode* pBoneNode = pCluster->GetLink();

		// get the bone ID for this bone name
		std::string strBoneName = std::tolower(pBoneNode->GetName());
				
		_log_action("Bone Table Size: " + std::to_string(boneTable.size()));

		int boneIndex = tools::GetIndexOf(strBoneName, boneTable);
		if (boneIndex == -1)
			return _log_action_error("Bone in FBX File: '" + strBoneName + "' is not found in skeleton ANIM file!");

		_log_action("Processing weighting for bone: '" + strBoneName + ", ID: " + std::to_string(boneIndex));

		// get the number of control point that this "bone" is affecting
		int controlPointIndexCount = pCluster->GetControlPointIndicesCount();

		if (controlPointIndexCount < 1)
		{
			_log_action_warning("No Influences for bone: "+ strBoneName+ "");
			continue;
		}		

		// get the indices and weights the current cluster (bone)
		int* pControlPointIndices = pCluster->GetControlPointIndices();
		double* pControlPointWeights = pCluster->GetControlPointWeights();

		if (!pControlPointIndices || !pControlPointWeights)
		{
			_log_action_error("NULL for bone weighted info!"); // TODO: make better message
			continue;
		}

		for (int influenceIndex = 0; influenceIndex < controlPointIndexCount; influenceIndex++)
		{
			// get control point 
			int controlPointIndex = pControlPointIndices[influenceIndex];

			// get weight associated with vertex
			double weight = pControlPointWeights[influenceIndex];	

			// run through all vertices
			for (int vertexIndex = 0; vertexIndex < destPackedMesh.vertices.size(); vertexIndex++)
			{
				// get the control point index for the current vertex
				auto controlPointIndexMesh = vertexToControlPoint[vertexIndex];
				
				// if it matches the rigging control point index, set weights
				if (controlPointIndex == controlPointIndexMesh)
				{
					destPackedMesh.vertices[vertexIndex].weightCount++;
					auto currentWeightIndex = destPackedMesh.vertices[vertexIndex].weightCount;

					destPackedMesh.vertices[vertexIndex].influences[currentWeightIndex-1].boneIndex = boneIndex;
					destPackedMesh.vertices[vertexIndex].influences[currentWeightIndex-1].weight = static_cast<float>(weight);
							
					auto DEBUG_BREAK = 1; // TODO: REMOVE!
				}
			}
			// add the influence {BONE, WEIGHT} to the proper vertex
			//m_vecvecTempControlPointInfluences[ctrl_point_index].push_back({ BONE_ID, weight });
			//vecMeshes[m_group].vecControlPoints[ctrl_point_index].addInfluence(0, 1.0f); // DEBUG CODE
		};
	}

	return true;
}

