using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonControls.ModelFiles.FBX
{
    abstract class IMarshaller<MANAGED_TYPE>
    {
        public virtual MANAGED_TYPE FromNative(IntPtr nativePtr)
        {
            throw new NotImplementedException();
        }
    }

    class StringMashaller : IMarshaller<string>
    {
        override public string FromNative(IntPtr nativePtr)
        {
            var managedDest = Marshal.PtrToStringUTF8(nativePtr);
            return managedDest;
        }
    }

    class MarshalHelper
    {
        private Dictionary<Type, object> _dictionryOfTypeToMashaller = new Dictionary<Type, object>();
        public MarshalHelper()
        {
            _dictionryOfTypeToMashaller[typeof(string)] = new StringMashaller();
        }

        static T Get<T>(IntPtr ptr) where T : class
        {
            var marhalHelper = new MarshalHelper();
            var marhsaller = marhalHelper.GetMarshaller<T>();
            return marhsaller.FromNative(ptr);
        }

        private IMarshaller<T> GetMarshaller<T>() where T : class
        {
            return _dictionryOfTypeToMashaller[typeof(T)] as IMarshaller<T>;
        }
    };

    //class test
    //{



    //    public static void TestMarhaller(IntPtr ptrString)
    //    {
    //        var marshaller = new MarshalHelper();
    //        string stringFromCpp = "";
    //        marshaller.Get<string>().FromNative(ptrString, ref stringFromCpp);
    //    }
    //}
};


//public class GenericDictionary
//    {


//        public void Add<T>(string key, T value) where T : class
//        {
//            _dict.Add(key, value);
//        }

//        public T GetValue<T>(string key) where T : class
//        {
//            return _dict[key] as T;
//        }
//    }

//}
