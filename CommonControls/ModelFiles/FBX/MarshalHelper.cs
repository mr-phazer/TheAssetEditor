using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CommonControls.ModelFiles.FBX.

namespace CommonControls.ModelFiles.FBX
{
    public abstract class IMarshalllerFancy<DEFINED_NATIVE_TYPE, MANAGED_TYPE>
    {

        public Dictionary<Type, IMarshalllerFancy<DEFINED_NATIVE_TYPE, MANAGED_TYPE>> dic = new Dictionary<Type, IMarshalller<DEFINED_NATIVE_TYPE, MANAGED_TYPE>>();;

        public IMarshalllerFancy()
        {
            dic[typeof(DEFINED_NATIVE_TYPE)] = this;
            _bIsBlitable = false;
        }
        public IMarshalllerFancy(bool isBlitable)
        {
            dic[typeof(DEFINED_NATIVE_TYPE)] = this;
            _bIsBlitable = isBlitable;
        }

        public virtual MANAGED_TYPE FromNative(IntPtr nativePtr)
        {
            throw new NotImplementedException();
        }

        private bool _bIsBlitable = false;
    }


    public class DXVector3Marhaller : IMarshalllerFancy<XMFLOAT3, Vector3>
    {
        public Vector3 FromNative(IntPtr nativePtr)
        {
            Vector3? pVector3;
            Marshal.Copy(pVector3, )
        }
    }


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
