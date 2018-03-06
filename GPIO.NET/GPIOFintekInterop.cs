using System.Runtime.InteropServices;

namespace renstech.GPIO.NET
{
    internal class GPIOFintekInterop
    {
        private const string DllPath = @"Fintek.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNowICID(ref uint param0);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPIO_LPC_W(int port, int value);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GPIO_LPC_R(int port, ref int value);
    }
}
