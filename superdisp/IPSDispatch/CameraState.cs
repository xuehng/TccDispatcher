namespace renstech.NET.SupernovaDispatcher.IPSDispatch
{
    internal class CameraState
    {
        public enum CameraType
        {
            FrontCamera,
            BackCamera,
        }

        public CameraState(CameraType type, bool isOn, uint faceAngle, uint titlAngel)
        {
            Type = type;
            IsOn = isOn;
            FaceAngle = faceAngle;
            TitlAngle = titlAngel;
        }

        public CameraType Type { get; private set; }
        public bool IsOn { get; private set; }
        public uint FaceAngle { get; private set; }
        public uint TitlAngle { get; private set; }
    }
}
