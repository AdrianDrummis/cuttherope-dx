using System;
using System.Runtime.InteropServices;

namespace CutTheRope.Desktop
{
    internal static class RetinaHelper
    {
        private static float? _cachedScaleFactor;

        public static float GetScaleFactor()
        {
            if (_cachedScaleFactor.HasValue)
            {
                return _cachedScaleFactor.Value;
            }

            float scale = 1.0f;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                scale = GetMacOSScaleFactor();
            }

            _cachedScaleFactor = scale;
            return scale;
        }

        private static float GetMacOSScaleFactor()
        {
            try
            {
                IntPtr nsScreenClass = objc_getClass("NSScreen");
                IntPtr mainScreenSel = sel_registerName("mainScreen");
                IntPtr backingScaleFactorSel = sel_registerName("backingScaleFactor");

                IntPtr mainScreen = objc_msgSend_IntPtr(nsScreenClass, mainScreenSel);
                if (mainScreen == IntPtr.Zero)
                {
                    return 1.0f;
                }

                double scaleFactor = objc_msgSend_double(mainScreen, backingScaleFactorSel);
                return (float)scaleFactor;
            }
            catch
            {
                return 1.0f;
            }
        }

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern double objc_msgSend_double(IntPtr receiver, IntPtr selector);
    }
}