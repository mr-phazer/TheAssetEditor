#include <iostream>

struct FBXControlPoint
{
	double x, y, z, w;
};

extern "C" __declspec(dllexport) int AddFromCppDll(int a, int b)
{
	return a + b;
};
