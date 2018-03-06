using System;
using com.renstech.SuperIPS.ipsevent;

namespace renstech.NET.nids.AsynEvent.ips
{
    public class EventSubscriber : AsynEventSubscriber
    {
        private string _locationTag;

        public string LocationTag
        {
            get { return _locationTag; }
            set
            {
                _locationTag = value;
                EventTags.Add(_locationTag);
            }
        }

        public event EventHandler<LocationEventArgs> OnLocationChanged;
        public event EventHandler<CameraStateArgs> OnCameraStateChanged; 

        private static int GetMessageTag(byte[] msg, ref string tag)
        {
            int index = Array.IndexOf(msg, (byte)0);
            if (index == -1)
                return -1;

            byte[] header = new byte[index];
            Array.Copy(msg, 0, header, 0, header.Length);

            try
            {
                tag = System.Text.Encoding.ASCII.GetString(header);
            }
            catch (Exception)
            {
                return -1;
            }
            return index;
        }

        private void HandleLocationUpdateEvent(IPSEventMessage message)
        {
            if (!message.HasLocationMsg)
                return;

            LocationMessage msg = message.LocationMsg;

            var args = new LocationEventArgs(msg.DeviceId, msg.MapId, msg.X, msg.Y);

            if (OnLocationChanged != null)
                OnLocationChanged(this, args);            
        }

        private void HandleCameraStateChangedEvent(IPSEventMessage message)
        {
            if (!message.HasCameraStateMsg)
                return;

            CameraStateMessage msg = message.CameraStateMsg;

            var args = new CameraStateArgs(msg.DeviceId,
                                           msg.IsCameraOn,
                                           msg.CameraType == CameraStateMessage.Types.CameraType.Front
                                               ? CameraStateArgs.CameraType.FrontCamera
                                               : CameraStateArgs.CameraType.BackCamera);
            args.FaceAngle = msg.FaceAngle;
            args.TiltAngle = msg.TiltAngle;

            if (OnCameraStateChanged != null)
                OnCameraStateChanged(this, args);
        }

        protected override void OnEventReceived(byte[] msg)
        {
            string tag = string.Empty;
            int index = GetMessageTag(msg, ref tag);
            if (index == -1)
                return;

            if (tag != LocationTag)
                return;

            byte[] body = new byte[msg.Length - index - 1];
            Array.Copy(msg, index+1, body, 0, body.Length);

            var message = IPSEventMessage.CreateBuilder().MergeFrom(body).Build();
            if (message == null)
                return;

            switch (message.MessageType)
            {
            case IPSEventMessage.Types.Type.CameraStateMsg:
                    HandleCameraStateChangedEvent(message);
                    break;
            case IPSEventMessage.Types.Type.LocationUpdateMsg:
                    HandleLocationUpdateEvent(message);
                    break;
            }
        }
    }
}