#include "FBXMeshGeomtryService.h"

std::map<std::string, std::vector<fbxsdk::FbxVector2>> FBXMeshGeometryService::LoadUVInformation(FbxMesh* pMesh)
{
	std::map<std::string, std::vector<fbxsdk::FbxVector2>> mapvecRet;

	std::map<int, int> test;

	//get all UV set names
	FbxStringList lUVSetNameList;
	pMesh->GetUVSetNames(lUVSetNameList);
	auto count = pMesh->GetElementUVCount();

	//iterating over all uv sets

	for (int lUVSetIndex = 0; lUVSetIndex < lUVSetNameList.GetCount(); lUVSetIndex++)
	{
		//get lUVSetIndex-th uv set
		const char* lUVSetName = lUVSetNameList.GetStringAt(lUVSetIndex);
		const FbxGeometryElementUV* lUVElement = pMesh->GetElementUV(lUVSetName);

		// init map vector, string -> UV list
		mapvecRet[lUVSetName] = std::vector<FbxVector2>();

		if (!lUVElement)
			continue;

		// only support mapping mode eByPolygonVertex and eByControlPoint
		if (lUVElement->GetMappingMode() != FbxGeometryElement::eByPolygonVertex &&
			lUVElement->GetMappingMode() != FbxGeometryElement::eByControlPoint)
		{
			/*for (auto& it : mapvecRet)
			{
				it.second.clear();
			}
			mapvecRet.clear();*/

			continue;
		}

		//index array, where holds the index referenced to the uv data
		const bool lUseIndex = lUVElement->GetReferenceMode() != FbxGeometryElement::eDirect;
		const int lIndexCount = (lUseIndex) ? lUVElement->GetIndexArray().GetCount() : 0;

		//iterating through the data by polygon
		const int lPolyCount = pMesh->GetPolygonCount();

		if (lUVElement->GetMappingMode() == FbxGeometryElement::eByControlPoint)
		{
			for (int lPolyIndex = 0; lPolyIndex < lPolyCount; ++lPolyIndex)
			{
				// build the max index array that we need to pass into MakePoly
				const int lPolySize = pMesh->GetPolygonSize(lPolyIndex);
				for (int lVertIndex = 0; lVertIndex < lPolySize; ++lVertIndex)
				{
					FbxVector2 lUVValue;

					//get the index of the current vertex in control points array
					int lPolyVertIndex = pMesh->GetPolygonVertex(lPolyIndex, lVertIndex);

					//the UV index depends on the reference mode
					int lUVIndex = lUseIndex ? lUVElement->GetIndexArray().GetAt(lPolyVertIndex) : lPolyVertIndex;

					lUVValue = lUVElement->GetDirectArray().GetAt(lUVIndex);

					// store
					mapvecRet[lUVSetName].push_back(lUVValue);
				}
			}
		}
		else if (lUVElement->GetMappingMode() == FbxGeometryElement::eByPolygonVertex)
		{
			int lPolyIndexCounter = 0;
			for (int lPolyIndex = 0; lPolyIndex < lPolyCount; ++lPolyIndex)
			{
				// build the max index array that we need to pass into MakePoly
				const int lPolySize = pMesh->GetPolygonSize(lPolyIndex);
				for (int lVertIndex = 0; lVertIndex < lPolySize; ++lVertIndex)
				{
					//if (lPolyIndexCounter < lIndexCount)
					{
						FbxVector2 lUVValue;

						//the UV index depends on the reference mode
						int lUVIndex = lUseIndex ? lUVElement->GetIndexArray().GetAt(lPolyIndexCounter) : lPolyIndexCounter;

						lUVValue = lUVElement->GetDirectArray().GetAt(lUVIndex);

						// store
						mapvecRet[lUVSetName].push_back(lUVValue);

						lPolyIndexCounter++;
					}
				}
			}
		}
	}

	return mapvecRet;
}
//get mesh normals info
std::vector<FbxVector4> FBXMeshGeometryService::GetNormals(fbxsdk::FbxMesh* poMesh, fbxsdk::FbxGeometryElementNormal::EMappingMode* poMappingMode)
{
	std::vector<FbxVector4> vecNormals;
	
	if (poMesh)
	{
		//get the normal element
		fbxsdk::FbxGeometryElementNormal* poNormalElement = poMesh->GetElementNormal();

		if (poNormalElement)
		{
			if (poMappingMode)
			{
				*poMappingMode = poNormalElement->GetMappingMode();
			}

			return FetchVectors(poMesh, poNormalElement);
		}
	}

	return vecNormals;
}

std::vector<FbxVector4> FBXMeshGeometryService::GetTangents(fbxsdk::FbxMesh* poMesh, fbxsdk::FbxGeometryElementNormal::EMappingMode* poMappingMode)
{
	std::vector<FbxVector4> vecTangents;

	if (poMesh)
	{
		//get the normal element
		fbxsdk::FbxGeometryElementTangent* pVectorElement = poMesh->GetElementTangent();

		if (pVectorElement)
		{
			if (poMappingMode)
			{
				*poMappingMode = pVectorElement->GetMappingMode();
			}

			return FetchVectors(poMesh, pVectorElement);
		}
	}

	return std::vector<FbxVector4>();
}

std::vector<FbxVector4> FBXMeshGeometryService::GetBitangents(fbxsdk::FbxMesh* poMesh, fbxsdk::FbxGeometryElementNormal::EMappingMode* poMappingMode)
{
	std::vector<FbxVector4> vecBinormals;

	if (poMesh)
	{
		//get the normal element
		fbxsdk::FbxGeometryElementBinormal* pVectorElement = poMesh->GetElementBinormal();

		if (pVectorElement)
		{
			if (poMappingMode)
			{
				*poMappingMode = pVectorElement->GetMappingMode();
			}

			return FetchVectors(poMesh, pVectorElement);
		}
	}

	return std::vector<FbxVector4>(); // return empty vector on errors
}

std::vector<FbxVector4> FBXMeshGeometryService::FetchVectors(fbxsdk::FbxMesh* poMesh, FbxLayerElementTemplate<FbxVector4>* poNormalElement)
{
	std::vector<FbxVector4> vecNormals;

	if (poMesh)
	{
		if (poNormalElement)
		{
			// Mapping mode is by Control Points. "The mesh should be smooth and soft."?
			// We can get normals by retrieving each control point
			if (poNormalElement->GetMappingMode() == fbxsdk::FbxGeometryElement::eByControlPoint)
			{
				int controlPointCount = poMesh->GetControlPointsCount();
				vecNormals.resize(controlPointCount);

				//Let's get normals of each vertex, since the mapping mode of normal element is by control point
				for (int vertexIndex = 0; vertexIndex < poMesh->GetControlPointsCount(); vertexIndex++)
				{
					int lNormalIndex = 0;

					// -- Reference mode is direct, the normal index is same as vertex index.
					// get normals by the index of control vertex
					if (poNormalElement->GetReferenceMode() == fbxsdk::FbxGeometryElement::eDirect)
						lNormalIndex = vertexIndex;

					// -- Reference mode is index-to-direct, get normals by the index-to-direct
					if (poNormalElement->GetReferenceMode() == fbxsdk::FbxGeometryElement::eIndexToDirect)
						lNormalIndex = poNormalElement->GetIndexArray().GetAt(vertexIndex);

					// -- Get Normal using the obtained index
					FbxVector4 lNormal = poNormalElement->GetDirectArray().GetAt(lNormalIndex);
					vecNormals[vertexIndex] = lNormal;
				}

			} // End: Vector ByControlPoint

			// Mapping mode is by polygon-vertex.
			// We can get normals by retrieving polygon-vertex.
			else if (poNormalElement->GetMappingMode() == FbxGeometryElement::eByPolygonVertex)
			{
				int lIndexByPolygonVertex = 0;
				int polygon_count = poMesh->GetPolygonCount();
				vecNormals.clear();
				//Let's get normals of each polygon, since the mapping mode of normal element is by polygon-vertex.
				for (int lPolygonIndex = 0; lPolygonIndex < polygon_count; lPolygonIndex++)
				{
					//get polygon size, you know how many vertices in current polygon.
					int lPolygonSize = poMesh->GetPolygonSize(lPolygonIndex);
					//retrieve each vertex of current polygon.
					for (int i = 0; i < lPolygonSize; i++)
					{
						int lNormalIndex = 0;
						// Reference mode is direct, the normal index is same as lIndexByPolygonVertex.
						if (poNormalElement->GetReferenceMode() == FbxGeometryElement::eDirect)
							lNormalIndex = lIndexByPolygonVertex;

						// Reference mode is index-to-direct, get normals by the index-to-direct
						if (poNormalElement->GetReferenceMode() == FbxGeometryElement::eIndexToDirect)
							lNormalIndex = poNormalElement->GetIndexArray().GetAt(lIndexByPolygonVertex);

						// Got normals of each polygon-vertex.
						FbxVector4 lNormal = poNormalElement->GetDirectArray().GetAt(lNormalIndex);
						lIndexByPolygonVertex++;

						vecNormals.push_back(lNormal); // TODO: check that index actually match as  like it does in the bellow out-commented code

						//vecNormals.resize(lIndexByPolygonVertex); // TODO: maybe just use push_back(), if the index will still match
						//vecNormals[lIndexByPolygonVertex - 1L] = lNormal;

					} // for i -> lPolygonSize

				} // lPolygonIndex -> PolygonCount

			} // end eByPolygonVertex

		} // end if lNormalElement

	} // end if lMesh

	return vecNormals;
}


