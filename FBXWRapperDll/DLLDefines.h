#pragma once

#ifdef FBXWRAPPERDLL_EXPORTS
#define FBXWRAPPERDLL_API __declspec(dllexport)
#else
#define FBXWRAPPERDLL_API __declspec(dllimport)
#endif

#ifdef FBXWRAPPERDLL_EXPORTS
#define FBXWRAPPERDLL_API_EXT extern "C" __declspec(dllexport)
#else
#define FBXWRAPPERDLL_API_EXT extern "C" __declspec(dllimport)
#endif

// base class for all "send pointer to C#" class, so they can deleted by the same function
class BaseObject 
{	
public:
	virtual ~BaseObject()
	{
		auto debug_break = 1;
	}	
};