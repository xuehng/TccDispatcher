using System.Collections.Generic;

namespace renstech.GPIO.NET
{
    internal class GPIOFintek
    {
        private ushort _deviceId;
        private readonly List<int> _pins = new List<int>();

        public bool Initialize()
        {
            for (int i = 0; i < 8; i++ )
                _pins.Add(0x30 + i);

            int value = 0;
            foreach (var pin in _pins)
            {
                value = SetPinMode(pin, true, ref value);
                SetPinOutputMode(pin, 0);
            }

            return true;
        }

        private int SetPinMode(int pin, bool inputPin, ref int value)
        {
            int iGpioVal = value;
            if (!inputPin)
            {
                switch (pin & 0x0F)
                {
                    case 0x00: iGpioVal = (iGpioVal | 0x01); break;
                    case 0x01: iGpioVal = (iGpioVal | 0x02); break;
                    case 0x02: iGpioVal = (iGpioVal | 0x04); break;
                    case 0x03: iGpioVal = (iGpioVal | 0x08); break;
                    case 0x04: iGpioVal = (iGpioVal | 0x10); break;
                    case 0x05: iGpioVal = (iGpioVal | 0x20); break;
                    case 0x06: iGpioVal = (iGpioVal | 0x40); break;
                    case 0x07: iGpioVal = (iGpioVal | 0x80); break;
                }
            }
            else
            {
                switch (pin & 0x0F)
                {
                    case 0x00: iGpioVal = (iGpioVal & 0xFE); break;
                    case 0x01: iGpioVal = (iGpioVal & 0xFD); break;
                    case 0x02: iGpioVal = (iGpioVal & 0xFB); break;
                    case 0x03: iGpioVal = (iGpioVal & 0xF7); break;
                    case 0x04: iGpioVal = (iGpioVal & 0xEF); break;
                    case 0x05: iGpioVal = (iGpioVal & 0xDF); break;
                    case 0x06: iGpioVal = (iGpioVal & 0xBF); break;
                    case 0x07: iGpioVal = (iGpioVal & 0x7F); break;
                }
            }

            switch (pin & 0xF0)
            {
                case 0x00: WirteGPIO(0xF0, iGpioVal); break;
                case 0x10: WirteGPIO(0xE0, iGpioVal); break;
                case 0x20: WirteGPIO(0xD0, iGpioVal); break;
                case 0x30: WirteGPIO(0xC0, iGpioVal); break;
                case 0x40: WirteGPIO(0xB0, iGpioVal); break;
                case 0x50: WirteGPIO(0xA0, iGpioVal); break;
            }

            return iGpioVal;            
        }

        void SetPinOutputMode(int index, int mode)
        {
            int iGpioVal = 0;
            switch (index & 0xF0)
            {
                case 0x00: iGpioVal = ReadGPIO(0xF1); break;
                case 0x10: iGpioVal = ReadGPIO(0xE1); break;
                case 0x20: iGpioVal = ReadGPIO(0xD1); break;
                case 0x30: iGpioVal = ReadGPIO(0xC1); break;
                case 0x40: iGpioVal = ReadGPIO(0xB1); break;
                case 0x50: iGpioVal = ReadGPIO(0xA1); break;
            }

            if (mode == 1)
            {
                switch (index & 0x0F)
                {
                    case 0x00: iGpioVal = (iGpioVal | 0x01); break;
                    case 0x01: iGpioVal = (iGpioVal | 0x02); break;
                    case 0x02: iGpioVal = (iGpioVal | 0x04); break;
                    case 0x03: iGpioVal = (iGpioVal | 0x08); break;
                    case 0x04: iGpioVal = (iGpioVal | 0x10); break;
                    case 0x05: iGpioVal = (iGpioVal | 0x20); break;
                    case 0x06: iGpioVal = (iGpioVal | 0x40); break;
                    case 0x07: iGpioVal = (iGpioVal | 0x80); break;
                }
            }
            else
            {
                switch (index & 0x0F)
                {
                    case 0x00: iGpioVal = (iGpioVal & 0xFE); break;
                    case 0x01: iGpioVal = (iGpioVal & 0xFD); break;
                    case 0x02: iGpioVal = (iGpioVal & 0xFB); break;
                    case 0x03: iGpioVal = (iGpioVal & 0xF7); break;
                    case 0x04: iGpioVal = (iGpioVal & 0xEF); break;
                    case 0x05: iGpioVal = (iGpioVal & 0xDF); break;
                    case 0x06: iGpioVal = (iGpioVal & 0xBF); break;
                    case 0x07: iGpioVal = (iGpioVal & 0x7F); break;
                }
            }

            switch (index & 0xF0)
            {
                case 0x00: WirteGPIO(0xF1, iGpioVal); break;
                case 0x10: WirteGPIO(0xE1, iGpioVal); break;
                case 0x20: WirteGPIO(0xD1, iGpioVal); break;
                case 0x30: WirteGPIO(0xC1, iGpioVal); break;
                case 0x40: WirteGPIO(0xB1, iGpioVal); break;
                case 0x50: WirteGPIO(0xA1, iGpioVal); break;
            }
        }

        public int ReadPin(int port)
        {
            int iGpioVal = 0;

            switch (port & 0xF0)
            {
                case 0x00: iGpioVal = ReadGPIO(0xF2); break;
                case 0x10: iGpioVal = ReadGPIO(0xE2); break;
                case 0x20: iGpioVal = ReadGPIO(0xD2); break;
                case 0x30: iGpioVal = ReadGPIO(0xC2); break;
                case 0x40: iGpioVal = ReadGPIO(0xB2); break;
                case 0x50: iGpioVal = ReadGPIO(0xA2); break;
            }

            if (((iGpioVal >> (port & 0x0F)) & 0x01) == 0x01)
                return 1;
            return 0;
        }

        private bool WirteGPIO(int port, int value)
        {
            int result = GPIOFintekInterop.GPIO_LPC_W(port, value);
            return (result != 0);
        }

        private int ReadGPIO(int port)
        {
            int value = 0;
            GPIOFintekInterop.GPIO_LPC_R(port, ref value);
            return value;
        }
    }
}
