#include "MeshProcessor.h"
#include "..\libs\meshopt\meshoptimizer.h"

//TODO: use meshopt, and add VertexRemapper variant of their code, that takes "CommonPackedVertex*"
// And processes the the tangents like in "DoMeshIndexingWithTangentSmoothing"
// Their hashing method is proper 10% faster, and it it MIT license 


// TODO: maybe also use this code, for meshes before Saving them as FBX?

void wrapdll::MeshProcessor::DoFinalMeshProcessing(PackedMesh& mesh)
{
    auto timedLogger = TimeLogAction::PrintStart("Do tangentBasis and indexing");
    //ComputeTangentBasisUnindexed(mesh.vertices);
    //DoMeshIndexingWithTangentSmoothing(mesh);


    DoTangentBasisAndIndexing(mesh);

    timedLogger.PrintDone();
}

void wrapdll::MeshProcessor::DoTangentBasisAndIndexing(PackedMesh& destMesh)
{
    using namespace std;
    using namespace DirectX;


    ComputeTangentBasisForUnindexedMeshWithRemp(/*vertexRemap, */destMesh.vertices);


    
    // TODO: , this is not reducing indexes meshes enough because, it is use the whole PackedCommonVertex"
    // "only use pos, normal, uv", "need to make temp struct()" "" and fille it upt


   /* struct SimpleVertex
    {
        sm::Vector4 pos;
        sm::Vector3 normal;
        sm::Vector2 uv;
    };
    std::vector<SimpleVertex> simpleVertices(destMesh.vertices.size());
    for (size_t i = 0; i < destMesh.vertices.size(); i++)
    {
        simpleVertices[i].pos = destMesh.vertices[i].position;
        simpleVertices[i].normal = destMesh.vertices[i].normal;
        simpleVertices[i].uv = destMesh.vertices[i].uv;
    }*/

    vector<uint32_t> vertexRemap(destMesh.vertices.size());
    auto newVertexCount = meshopt_generateVertexRemapTBN(
        vertexRemap.data(),
        &destMesh.indices[0],
        destMesh.indices.size(),
        destMesh.vertices.data(),
        destMesh.vertices.size(),
        sizeof(PackedCommonVertex)

    );

    
    

    std::vector<uint32_t> indexRemap(newVertexCount);
    meshopt_remapIndexBuffer(destMesh.indices.data(), NULL, destMesh.indices.size(), indexRemap.data());
    meshopt_remapVertexBuffer(destMesh.vertices.data(), destMesh.vertices.data(), destMesh.vertices.size(), sizeof(PackedCommonVertex), vertexRemap.data());



    // -- fill the mesh with the proceessed data

    for (auto& v : destMesh.vertices)
    {
        v.position.w = 0;
        v.normal = sm::Vector3::Normalize(v.normal);
        std::swap(v.tangent, v.bitangent);
        v.tangent = sm::Vector3::Normalize(v.tangent);
        v.bitangent = sm::Vector3::Normalize(v.bitangent);
    }

    RemapVertexWeights<uint32_t, ~0u>(destMesh.vertexWeights, destMesh.vertexWeights,  vertexRemap);

    //for (size_t i = 0; i < destMesh.vertices.size(); i++)
    //{
    //    auto& v = destMesh.vertices[i];
    //    auto& v_src = destMesh.vertices[i];
    //    v.position = XMFLOAT4(outVertices[i].x, outVertices[i].y, outVertices[i].z, 0);

    //    outNormals[i].Normalize();
    //    v.normal = outNormals[i];

    //    outTangents[i].Normalize();
    //    outBitangents[i].Normalize();

    //    v.tangent = outTangents[i];
    //    v.bitangent = outBitangents[i];

    //    v.uv = outUVs[i];
    //}

    //destMesh.indices = outIndices;
}

template <typename T, T DISCARD_VALUE>
void wrapdll::MeshProcessor::RemapVertexWeights(const std::vector<VertexWeight>& inVertexWeights, std::vector<VertexWeight>& outVertexWeights, const std::vector<T>& outVertexIndexRemap)
{
    for (size_t i = 0; i < inVertexWeights.size(); i++) // run through all old vertexweights
    {
        if (outVertexIndexRemap[inVertexWeights[i].vertexIndex] != DISCARD_VALUE)
        {
            auto tempVertexWeight = inVertexWeights[i];

            // remap the index, 
            tempVertexWeight.vertexIndex = outVertexIndexRemap[inVertexWeights[i].vertexIndex];

            // store the weight
            outVertexWeights.push_back(tempVertexWeight); 
        }
    }
}