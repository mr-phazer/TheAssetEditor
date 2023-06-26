using System;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential)]
class FBXControlPoint
{
    public double x;
    public double Y;
    public double z;
    public double w;
}

    class TestClass
{
    // Import the DLL
    [DllImport("K:\\Coding\\repos\\TheAssetEditor\\x64\\Debug\\InteropTestDll.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddFromCppDll(int a, int b);

    static void Main(string[] args)   
    {       
      
        // Call the DLL function
        int result = AddFromCppDll(3, 4);
        Console.WriteLine(result);        
    }
}