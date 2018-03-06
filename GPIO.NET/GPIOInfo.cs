#define FINTEK

namespace renstech.GPIO.NET
{
    public class GPIOInfo
    {
#if FINTEK
        private GPIOFintek _gpioInst;
#endif
        public bool Initialize()
        {

#if FINTEK
            _gpioInst = new GPIOFintek();
            _gpioInst.Initialize();
#elif GPIO6854
            Gpio6854.GPIO_Initialize();
#endif
            return true;
        }

        public void UpdateGPIO()
        {

#if FINTEK
            return;
#elif GPIO6854
            Gpio6854.GPIO_Scan();
#endif
        }

        public int ReadGPIO(int port)
        {

#if FINTEK
            return _gpioInst.ReadPin(port);
#elif GPIO6854
            return Gpio6854.GPIO_GetPortStatus(port);
#endif
        }
    }
}
