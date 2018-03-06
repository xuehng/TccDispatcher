using System;

namespace renstech.NET.nids.AsynEvent.ips
{
    public class CameraStateArgs : EventArgs
    {
        #region CameraType enum

        public enum CameraType
        {
            FrontCamera,
            BackCamera
        };

        #endregion

        public CameraStateArgs(ulong devId, bool on, CameraType type)
        {
            DeviceId = devId;
            IsCameraOn = on;
            CamType = type;
        }

        public ulong DeviceId { get; private set; }
        public bool IsCameraOn { get; private set; }
        public CameraType CamType { get; private set; }
        public uint FaceAngle { get; set; }
        public uint TiltAngle { get; set; }
    }
}