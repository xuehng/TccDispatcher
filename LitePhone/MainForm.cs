using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using renstech.NET.PJSIP;
using renstech.NET.SIPUA;
using renstech.NET.InterProcPipe;

namespace renstech.sharpsip
{
    public partial class MainForm : Form
    {
        private SIPUA _ua = new SIPUA();
        private int     _accountId;
        private int     _callId;
        private int     _recordId;
        private bool    _runBackground;
        private string  _recordDir;

        private string _pipeName;
        private NamedPipeClient _pipe = null;

        public MainForm(string[] args)
        {
            InitializeComponent();
            regStateButton.BackColor = Color.DarkGray;

            _accountId = -1;
            _callId = -1;
            _recordId = -1;
            _runBackground = false;

            ParseArgs(args);

            labelIncomeMsgQueu.Text = _pipeName;

            if (!string.IsNullOrEmpty(_pipeName))
            {
                _pipe = new NamedPipeClient(_pipeName);
                _pipe.OnReceivedMessage += OnReceiveMsg;
                _pipe.Start();

                HandsetNotifyMsg msg = new HandsetNotifyMsg();
                msg.MsgNotifyCode = HANDSETMSG_NOTIFY_CODE.MSG_NOTIFY_ONLINE;
                SendMessage(msg);
            }

            if (_runBackground)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void ParseArgs(string[] args)
        {
            string argsSingle = String.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                //管道名
                if (args[i] == "--pipe")
                    _pipeName = args[i + 1];
                argsSingle += args[i];
                argsSingle += " ";
            }

            //后台运行
            _runBackground = true;

            //录音目录
            _recordDir = argsSingle.Substring(26, argsSingle.Length - 34);
            //MessageBox.Show(String.Format("{0}", _recordDir));
        }

        private void OnReceiveMsg(object sender, ReceivedMessageEventArgs args)
        {
            HandsetReqMsg request = JsonConvert.DeserializeObject<HandsetReqMsg>(args.Message);
            if (request == null)
            {
                return;
            }

            switch (request.MsgRequestCode)
            {
                case HANDSETMSG_REQ_CODE.MSG_REQ_RUN:
                    HandsetReqUAStartMsg run = JsonConvert.DeserializeObject<HandsetReqUAStartMsg>(args.Message);
                    if (run != null)
                    {
                        RunUADelegate d = new RunUADelegate(RunUA);
                        this.BeginInvoke(d, new object[] { run.CaptureDevice, run.PlaybackDevice });
                    }
                    break;

                case HANDSETMSG_REQ_CODE.MSG_REQ_ADDACCOUNT:
                    HandsetReqAddAccountMsg account = JsonConvert.DeserializeObject<HandsetReqAddAccountMsg>(args.Message);
                    if (account != null)
                    {
                        AddAccountDelegate d = new AddAccountDelegate(AddAccount);
                        this.BeginInvoke(d, new object[] { account.User, account.Password, account.Domain, account.Proxy, account.IsReg, account.IsAutoAnswer });
                    }
                    break;
                case HANDSETMSG_REQ_CODE.MSG_REQ_ANSWER:
                    {
                        AnswerDelegate d = new AnswerDelegate(Answer);
                        this.BeginInvoke(d);
                    }
                    break;
                case HANDSETMSG_REQ_CODE.MSG_REQ_HANGUP:
                    {
                        HangupDelegate d = new HangupDelegate(Hangup);
                        this.BeginInvoke(d);
                    }
                    break;
                case HANDSETMSG_REQ_CODE.MSG_REQ_MAKECALL:
                    HandsetReqMakeCallMsg call = JsonConvert.DeserializeObject<HandsetReqMakeCallMsg>(args.Message);
                    if (call != null)
                    {
                        MakeCallDelegate d = new MakeCallDelegate(MakeCall);
                        this.BeginInvoke(d, new object[] { call.DestUri });
                    }
                    break;
                case HANDSETMSG_REQ_CODE.MSG_REQ_RECORDINGDIRCHANGED:
                    //更改录音文件的存放路径
                    HandsetReqRecordingDirChangedMsg recordingDir = JsonConvert.DeserializeObject<HandsetReqRecordingDirChangedMsg>(args.Message);
                    if (recordingDir != null)
                        _recordDir = recordingDir.newRecordingDir;
                    break;
            }
        }

        private void SendMessage(HandsetMsg msg)
        {
            string output = JsonConvert.SerializeObject(msg);
            if (_pipe != null)
                _pipe.Write(output);
        }

        # region sip ua operation function
        private delegate void RunUADelegate(int capture, int playbak);
        private void RunUA(int capture, int playbak)
        {
            _ua.CaptureDeviceId = capture;
            _ua.PlaybakDeviceId = playbak;
            _ua.Start();

            _ua.IncomingCallInfo += on_incoming_call;
            _ua.RegStateInfo += on_reg_state_changed;
            _ua.CallStateInfo += on_call_state;

            HandsetNotifyInfoMsg msg = new HandsetNotifyInfoMsg();
            msg.host_addr = _ua.GetHostAddr();
            msg.udp_port = _ua.GetUdpPort();
            SendMessage(msg);
        }

        private delegate void AddAccountDelegate(string user, string password, string domain, string proxy, bool isreg, bool isautoanswer); 
        private void AddAccount(string user, string password, string domain, string proxy, bool isreg, bool isautoanswer)
        {
            Account account = new Account();
            account.User = user;
            account.Password = password;
            account.Domain = domain;
            account.IsDomainRegistration = true;
            account.IsRegistrationEnabled = false;
            account.IsAutoAnswer = isautoanswer;

            if (!string.IsNullOrEmpty(proxy))
            {
                account.IsDomainRegistration = false;
                account.Proxy = proxy;
            }
            _ua.AddAccount(account);
            _accountId = account.Id;

            if (!_runBackground)
            {
                txtUser.Text = user;
                txtPassword.Text = password;
                txtDomain.Text = domain;
                txtProxy.Text = proxy;
                checkBoxReg.Checked = isreg;
                checkBoxAutoAnswer.Checked = isautoanswer;
            }
        }

        private delegate void AnswerDelegate();
        private void Answer()
        {
            if (_callId != -1)
                _ua.Answer_NoVideo(_callId);
        }

        private delegate void HangupDelegate();
        private void Hangup()
        {
            if (_callId != -1)
                _ua.Hangup(_callId);
        }

        private delegate void MakeCallDelegate(string dest);
        private void MakeCall(string dest)
        {
            if (_accountId != -1)
                _ua.MakeCall_NoVideo(_accountId, dest);
        }
        
        private void StartCallRecording()
        {
            //MessageBox.Show("Recording start");
            if (_callId == -1 || _recordId != -1 || string.IsNullOrEmpty(_recordDir) )
            {
                //MessageBox.Show("Record Wrong, return");
                return;
            }

            Account account = _ua.GetAccount(_accountId);

            sua_call_info info = new sua_call_info();
            if (!_ua.GetCallInfo(_callId, ref info))
            {
                //MessageBox.Show("Record Wrong, return");
                return;
            }

            SIPUri uri = new SIPUri(info.remote_uri);
            string remoteNum = uri.User;

            string filename = null;
            if (((sua_role_e)info.call_role) == sua_role_e.SUA_ROLE_UAS)
            {
                filename = string.Format("{0}\\{1}-{2}-{3}.wav", _recordDir,
                    DateTime.Now.ToString("yyyyMMddHHmmss"), remoteNum, account.User);
            }
            else
            {
                filename = string.Format("{0}\\{1}-{2}-{3}.wav", _recordDir,
                    DateTime.Now.ToString("yyyyMMddHHmmss"), account.User, remoteNum);
            }

            //MessageBox.Show(String.Format("FileName: {0}, callid: {1}",filename, _callId));

            filename = filename.Replace("*", "γ");

            var result = _ua.StartRecording(_callId, filename, ref _recordId);

            //MessageBox.Show(String.Format("Start record result: {0}", result));
        }

        private void StopRecordCall()
        {
            //MessageBox.Show("Record stop");
            if (_recordId != -1)
            {
                _ua.StopRecord(_recordId);
                _recordId = -1;
            }
        }

        #endregion sip ua operation function

        # region sip ua callback
        private delegate void OnLogUIDelegate(string log);
        private void OnUIThreadLogUpdate(string log)
        {
            Debug.WriteLine(log);
        }

        private void on_log_update(object sender, LogEventArgs e)
        {
            OnLogUIDelegate d = new OnLogUIDelegate(OnUIThreadLogUpdate);
            if (this.IsHandleCreated)
                this.BeginInvoke(d, e.Data);
        }

        private delegate void OnIncomingCallUIDelegate(string remote);
        private void onUIIncomingCall(string remote)
        {
            btnChannel1.Text = remote;
        }

        private void on_incoming_call(object sender, IncomingCallArgs e)
        {
            if (_callId != -1 && _callId != e.CallId)
            {
                Hangup();
            }

            _callId = e.CallId;

            Account account = _ua.GetAccount(_accountId);
            if (account.IsAutoAnswer)
            {
                Answer();
            }

            OnIncomingCallUIDelegate d = new OnIncomingCallUIDelegate(onUIIncomingCall);
            this.BeginInvoke(d, new object[] { e.RemoteUri });
        }

        private delegate void OnRegStateUIDelegate(int statusCode);
        private void OnUIThreadRegStateChange(int status)
        {
            regStateButton.BackColor = (status == 200) ? Color.DarkGreen : Color.DarkGray;
        }

        private void on_reg_state_changed(object sender , RegStateArgs e)
        {     
            OnRegStateUIDelegate regstate = new OnRegStateUIDelegate(OnUIThreadRegStateChange);
            this.BeginInvoke(regstate, new object[] { e.StatusCode });
        }

        private delegate void OnCallStateUIDelegate(int call_id, sua_inv_state state);
        private void OnUICallStateChanged(int call_id, sua_inv_state state)
        {
            if (_callId != -1 && _callId != call_id)
            {
                Hangup();
            }

            sua_call_info info = new sua_call_info();
            PJSIPInterop.sua_call_get_info(call_id, ref info);

            switch ((sua_inv_state)state)
            {
                case sua_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    _callId = -1;
                    btnChannel1.Text = "";
                    StopRecordCall();
                    break;
                case sua_inv_state.PJSIP_INV_STATE_INCOMING:
                    _callId = call_id;
                    btnChannel1.Text = info.remote_uri;
                    break;
                case sua_inv_state.PJSIP_INV_STATE_CALLING:
                    _callId = call_id;
                    btnChannel1.Text = info.remote_uri;
                    break;
                case sua_inv_state.PJSIP_INV_STATE_CONFIRMED:                 
                    StartCallRecording();
                    break;
            }

            HandsetNotifyCallStateMsg msg = new HandsetNotifyCallStateMsg();
            msg.remoteUri = info.remote_uri;
            msg.role = info.call_role;
            msg.state = (int)state;
            SendMessage(msg);
        }

        private void on_call_state(object sender, CallStateArgs e)
        {
            OnCallStateUIDelegate callstate = new OnCallStateUIDelegate(OnUICallStateChanged);
            this.BeginInvoke(callstate, new object[] { e.CallId, e.STATE });
        }
        #endregion sip ua callback

        private void dialButton_Click(object sender, EventArgs e)
        {
            _ua.MakeCall(_accountId, dialStringTextBox.Text, 0);
        }

        private void answerButton_Click(object sender, EventArgs e)
        {
            Answer();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void hangupButton_Click(object sender, EventArgs e)
        {
            Hangup();
        }

        private void btnStartUA_Click(object sender, EventArgs e)
        {
            string User = txtUser.Text;
            string Password = txtPassword.Text;
            string Domain = txtDomain.Text;
            string Proxy = txtProxy.Text;

            RunUA(-1, -1);
            AddAccount(User, Password, Domain, Proxy, checkBoxReg.Checked, false);
        }

        private void regButton_Click(object sender, EventArgs e)
        {
            if (_accountId != -1)
                _ua.SetAccountRegistration(_accountId, true);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (_runBackground)
                this.Hide();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ua.Stop();
        }
    }
}