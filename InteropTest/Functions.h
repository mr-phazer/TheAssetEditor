#include <iostream>
#include <malloc.h>

struct FBXControlPoint
{
	double x;
	double y;
	double z;
	double w;
};

//extern "C" __declspec(dllexport) int AddFromCppDll(int a, int b)
//{
//	return a + b;
//};
//

extern "C" __declspec(dllexport) int CreateMyData(int length)
{
	/**outData = (FBXControlPoint*)malloc(sizeof(FBXControlPoint));

	(*outData)->x = 1;
	(*outData)->y = 2;
	(*outData)->z = 2;
	(*outData)->w = 3;*/

	return 1;
}

//extern "C" __declspec(dllexport) FBXControlPoint * GetControlPoint()
//extern "C" __declspec(dllexport) void API_ReadFile(FBXControlPoint* outData)
//{
//	static FBXControlPoint p;
//	(*outData).x = 1;
//	(*outData).y = 2;
//	(*outData).z = 2;
//	(*outData).w = 3;
//};
