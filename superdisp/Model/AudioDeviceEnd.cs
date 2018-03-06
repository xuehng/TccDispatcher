using CoreAudioApi;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class AudioDeviceEnd
    {
        private MMDeviceCollection _collections = null;

        public AudioDeviceEnd()
        {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            _collections = DevEnum.EnumerateAudioEndPoints(EDataFlow.eAll, EDeviceState.DEVICE_STATE_ACTIVE);
        }

        private MMDevice GetDevice(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < _collections.Count; i++)
            {
                if (string.Compare(_collections[i].Name, 0, name, 0, name.Length) == 0)
                    return _collections[i];
            }
            return null;
        }

        public bool GetDeviceVolume(string name, ref float value)
        {
            MMDevice device = GetDevice(name);
            if (device == null)
                return false;

            value = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            return true;
        }

        public bool SetDeviceVolume(string name, float value)
        {
            MMDevice device = GetDevice(name);
            if (device == null)
                return false;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = value;
            return true;
        }
    }
}
