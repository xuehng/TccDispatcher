using System;
using System.Collections.Generic;
using System.Linq;
using renstech.NET.nids.AsynEvent;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class DialogInfo
    {
        public string ChannelUuid { get; set; }

        /// <summary>
        /// 主叫号码
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 主叫显示名称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 被叫号码
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 被叫显示名称
        /// </summary>
        public string ToName { get; set; }

        public bool ALegDialog { get; set; }
        public ChannelEvent DialogState { get; set; }

        public bool IsDialogRinging { get { return (DialogState == ChannelEvent.CHANNEL_OUTGOING); } }
        public bool IsDialogAnswered { get { return (DialogState == ChannelEvent.CHANNEL_ANSWERED); } }
        public bool IsDialogHanguped
        {
            get
            {
                return (DialogState == ChannelEvent.CHANNEL_HANGUP ||
                        DialogState == ChannelEvent.CHANNEL_DESTROY);
            }
        }

        public override string ToString()
        {
            return string.Format("DialogInfo(" +
                                 "uuid: {0}, from: {1}, to: {2}, " +
                                 "ring: {3}, answer: {4}, hangup: {5})",
                                 ChannelUuid, FromName, ToName, 
                                 IsDialogRinging, IsDialogAnswered, IsDialogHanguped);
        }
    }

    public class DialogInfoDb
    {
        private static readonly object LockObject = new object();

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DialogInfoDb));

        /// <summary>
        /// Key：uuid，Value：Dialog
        /// </summary>
        readonly Dictionary<string, DialogInfo> _uuidToDialog = new Dictionary<string, DialogInfo>();

        /// <summary>
        /// Key：User.Number（主叫），Value：uuid列表
        /// </summary>
        readonly Dictionary<string, List<string>> _dictCallerInfo = new Dictionary<string, List<string>>();

        /// <summary>
        /// Key：User.Number（被叫）， Value：uuid列表
        /// </summary>
        readonly Dictionary<string, List<string>> _dictCalleeInfo = new Dictionary<string, List<string>>();


        /// <summary>
        /// 获取Dialog列表
        /// </summary>
        public Dictionary<string, DialogInfo> GetAllDialog()
        {
            return _uuidToDialog;
        }

        /// <summary>
        /// 如果列表中存在与新Dialog的uuid相同的Dialog，则更新之。否则添加至列表
        /// </summary>
        /// <param name="dialog">新收到的Dialog</param>
        public void HandleDialogUpdate(DialogInfo dialog)
        {
            lock (LockObject)
            {
                DialogInfo info = GetDialogByUuid(dialog.ChannelUuid);
                if (info == null)
                {
                    if (dialog.DialogState == ChannelEvent.CHANNEL_DESTROY)
                    {
                        Log.Debug("HandleDialogUpdate, got destroy event, and no dialogInfo exist. RETURN");
                        return;
                    }
                    Log.Debug("HandleDialogUpdate__info == null, gonna add dialog");
                    AddDialog(dialog);
                }
                else
                {
                    Log.Debug("HandleDialogUpdate__info != null, gonna update dialog");
                    UpdateDialog(dialog.ChannelUuid, dialog.DialogState);
                }
            }
        }

        /// <summary>
        /// 如果不是Hangup、Destroy的Dialog，则将其添加至列表。
        /// </summary>
        public void AddDialog(DialogInfo dialog)
        {
            if (dialog.IsDialogHanguped)
            {
                Log.Error(string.Format("AddDialog__dialog.IsDialogHanguped, which shouldn't be happening. " +
                                        "gonna try remove anyway. dialog: {0}. RETURN", dialog));
                RemoveDialog(dialog.ChannelUuid);
                return;
            }

            _uuidToDialog.Add(dialog.ChannelUuid, dialog);
            Log.Debug(string.Format("AddDialog, overAll dict added. " +
                                    "dialog: {0}, curCount: {1}", dialog, _uuidToDialog.Count));

            if (dialog.ALegDialog)
            {
                if (!_dictCallerInfo.ContainsKey(dialog.From) ||
                     _dictCallerInfo[dialog.From] == null)
                {
                    _dictCallerInfo[dialog.From] = new List<string>();
                }
                _dictCallerInfo[dialog.From].Add(dialog.ChannelUuid);
               
                Log.Debug(string.Format("AddDialog, caller dict added. " +
                                        "caller: {0}, dialog: {1}, callerCnt: {2}", dialog.From, dialog, _dictCallerInfo.Count));
            }
            else
            {
                if (!_dictCalleeInfo.ContainsKey(dialog.To) ||
                    _dictCalleeInfo[dialog.To] == null)
                {
                    _dictCalleeInfo[dialog.To] = new List<string>();
                }
                _dictCalleeInfo[dialog.To].Add(dialog.ChannelUuid);

                Log.Debug(string.Format("AddDialog, callee dict added. " +
                                        "callee: {0}, dialog: {1}, calleeCnt: {2}", dialog.To, dialog.ChannelUuid, _dictCalleeInfo.Count));
            }
        }

        public void RemoveDialog(string uuid)
        {
            if (!_uuidToDialog.ContainsKey(uuid))
            {
                Log.Error(string.Format("RemoveDialog__!uuidToDialog.ContainsKey(uuid), which shouldn't be happening." +
                                        "uuid: {0}. RETURN", uuid));
                return;
            }
            
            DialogInfo info = _uuidToDialog[uuid];
            _uuidToDialog.Remove(uuid);
            Log.Debug(string.Format("RemoveDialog, overAll dialog removed. " +
                                    "dialog: {0}, cnt: {1}", info, _uuidToDialog.Count));

            if (info.ALegDialog)
            {
                if (_dictCallerInfo.ContainsKey(info.From) &&
                    _dictCallerInfo[info.From] != null)
                {
                    Log.Debug(string.Format("RemoveDialog, caller dict removed. " +
                                            "caller: {0}, dialog: {1}, callerCnt: {2}", info.From, _dictCallerInfo[info.From], _dictCallerInfo.Count));
                    
                    _dictCallerInfo[info.From].Remove(uuid);
                }
            }
            else
            {
                if (_dictCalleeInfo.ContainsKey(info.To) &&
                    _dictCalleeInfo[info.To] != null)
                {
                    Log.Debug(string.Format("RemoveDialog, callee dict removed. " +
                                            "callee: {0}, dialog: {1}, calleCnt: {2}", info.To, _dictCalleeInfo[info.To], _dictCalleeInfo.Count));

                    _dictCalleeInfo[info.To].Remove(uuid);
                }
            }
        }

        /// <summary>
        /// 根据uuid，更新列表中对应Dialog的DialogState。如果为Hangup、Destroy，则从列表中移除
        /// </summary>
        private void UpdateDialog(string uuid, ChannelEvent state)
        {
            DialogInfo info = GetDialogByUuid(uuid);
            if (info == null)
            {
                Log.Error(string.Format("UpdateDialog__info == null. uuid: {0}, state: {1}", uuid, state));
                return;
            }

            info.DialogState = state;

            if (info.IsDialogHanguped)
            {
                Log.Debug(string.Format("UpdateDialog__info.IsDialogHanguped, goona remove. uuid: {0}", uuid));
                try
                {
                    RemoveDialog(uuid);
                }
                catch (Exception e)
                {
                   Log.Warn(string.Format("UpdateDialog__info.IsDialogHanguped, failed to remove. uuid: {0}", uuid)); 
                }
            }
            else
            {
                Log.Debug(string.Format("UpdateDialog__!info.IsDialogHanguped. uuid: {0}", uuid));
            }
        }

        /// <summary>
        /// 通过uuid，获得对应的Dialog
        /// </summary>
        /// <param name="uuid">目标Dialog的uuid</param>
        /// <returns>找到返回Dialog，否则返回Null</returns>
        public DialogInfo GetDialogByUuid(string uuid)
        {
            return !_uuidToDialog.ContainsKey(uuid) ? null : _uuidToDialog[uuid];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public DialogInfo GetCallerDialog(string number, string prefix)
        {
            if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(prefix))
            {
                Log.Error(string.Format("GetCallerDialog, param invalid. number: {0}, prefix: {1}",
                    number ?? string.Empty, prefix ?? string.Empty));
                return null;
            }

            if (number.StartsWith(prefix))
            {
                return GetCallerDialog(number) ?? GetCallerDialog(number.Substring(prefix.Length));
            }

            return GetCallerDialog(number);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public DialogInfo GetCalleeDialog(string number, string prefix)
        {
            if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(prefix))
            {
                Log.Error(string.Format("GetCalleeDialog, param invalid. number: {0}, prefix: {1}",
                    number ?? string.Empty, prefix ?? string.Empty));
                return null;
            }

            if (number.StartsWith(prefix))
            {
                return GetCalleeDialog(number) ?? GetCalleeDialog(number.Substring(prefix.Length));
            }

            return GetCalleeDialog(number);
        }

        /// <summary>
        /// for caller dialog, only return a leg dialog info
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private DialogInfo GetCallerDialog(string caller)
        {
            if (!_dictCallerInfo.ContainsKey(caller))
            {
                Log.Error(string.Format("GetCallerDialog__!_dictCallerInfo.ContainsKey(caller). " +
                                        "caller: {0}. RETURN NULL.",caller));
                return null;
            }
            if (_dictCallerInfo[caller] == null)
            {
                Log.Warn(string.Format("GetCallerDialog___dictCallerInfo[caller] == null. " +
                                        "caller: {0}. RETURN NULL.", caller));
                return null;
            }
            if (_dictCallerInfo[caller].Count == 0)
            {
                Log.Warn(string.Format("GetCallerDialog___dictCallerInfo[caller].Count == 0. " +
                                         "caller: {0}. RETURN NULL.", caller));
                return null;
            }

            string channelUuid = _dictCallerInfo[caller].ElementAt(_dictCallerInfo[caller].Count - 1);
            return GetDialogByUuid(channelUuid);
        }

        /// <summary>
        /// return b leg dialog info
        /// </summary>
        /// <param name="callee"></param>
        /// <returns></returns>
        private DialogInfo GetCalleeDialog(string callee)
        {
            if (!_dictCalleeInfo.ContainsKey(callee))
            {
                Log.Error(string.Format("GetCalleeDialog__!_dictCalleeInfo.ContainsKey(callee). " +
                                        "callee: {0}. RETURN NULL.", callee));
                return null;
            }
            if (_dictCalleeInfo[callee] == null)
            {
                Log.Warn(string.Format("GetCalleeDialog___dictCalleeInfo[callee] == null. " +
                                        "callee: {0}. RETURN NULL.", callee));
                return null;
            }
            if (_dictCalleeInfo[callee].Count == 0)
            {
                Log.Warn(string.Format("GetCalleeDialog___dictCalleeInfo[callee].Count == 0. " +
                                         "callee: {0}. RETURN NULL.", callee));
                return null;
            }

            string channelUuid = _dictCalleeInfo[callee].ElementAt(_dictCalleeInfo[callee].Count - 1);
            return GetDialogByUuid(channelUuid);
        }

        public void ClearAllDialog()
        {
            _dictCallerInfo.Clear();
            _dictCalleeInfo.Clear();
            _uuidToDialog.Clear();
        }
    }
}