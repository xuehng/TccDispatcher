using System.Runtime.InteropServices;

namespace renstech.GPIO.NET
{
    public class Gpio6854Interop
    {
        private const string DllPath = @"GPIO6854.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPIO_Initialize();

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GPIO_Scan();

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPIO_GetPortStatus(int port);
    }
}
