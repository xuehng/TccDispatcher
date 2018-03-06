using System;
using System.Windows;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Layout
{
	/// <summary>
	/// GroupContextWindow.xaml 的交互逻辑
	/// </summary>
	public partial class GroupContextWindow : Window
	{
		Group _group = null;
		SpnvSubSystem _system = null;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GroupContextWindow));

		public GroupContextWindow(Group group, SpnvSubSystem system)
		{
			this.InitializeComponent();

			DataContext = group;

			_group = group;
			_system = system;

			App app = App.Current as App;
			if (app.AppBkBrush != null)
				Background = app.AppBkBrush;
		}

		private void btnGroupPaging_Click(object sender, RoutedEventArgs e)
		{
            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupPaging_Click, Start");

			string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialPaging);
			if (string.IsNullOrEmpty(prefix))
				return;

			string grpstr = _system.PrefixInfo.GetGroupDialString(_group);
			string dest = string.Format("{0}{1}", prefix, grpstr);

			int callId = -1;
			_system.Channels.MakeCall(_system.AccountId, dest, ref callId, true);

            Log.Debug(String.Format("Protocal Stack Log: (GroupContextWindow)btnGroupPaging_Click, Make Call:{0},{1},{2},{3}", _system.AccountId, dest, callId, true));

            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupPaging_Click, End");

			DialogResult = true;
		}

		private void btnGroupConf_Click(object sender, RoutedEventArgs e)
		{
            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupConf_Click, Start");

			string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialConference);
			if (string.IsNullOrEmpty(prefix))
				return;

			string grpstr = _system.PrefixInfo.GetGroupDialString(_group);
			string dest = string.Format("{0}{1}", prefix, grpstr);

			int callId = -1;
			_system.Channels.MakeCall(_system.AccountId, dest, ref callId, true);

            Log.Debug(String.Format("Protocal Stack Log: (GroupContextWindow)btnGroupConf_Click, Make Call:{0},{1},{2},{3}", _system.AccountId, dest, callId, true));

            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupConf_Click, End");

			DialogResult = true;
		}

		private void btnGroupFind_Click(object sender, RoutedEventArgs e)
		{
            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupFind_Click, Start");

			string prefix = _system.PrefixInfo.GetPrefix(DialPrefixType.DialFindcall);
			if (string.IsNullOrEmpty(prefix))
				return;

			string grpstr = _system.PrefixInfo.GetGroupDialString(_group);
			string dest = string.Format("{0}{1}", prefix, grpstr);

			int callId = -1;
			_system.Channels.MakeCall(_system.AccountId, dest, ref callId, true);

            Log.Debug(String.Format("Protocal Stack Log: (GroupContextWindow)btnGroupFind_Click, Make Call:{0},{1},{2},{3}", _system.AccountId, dest, callId, true));

            Log.Debug("Protocal Stack Log: (GroupContextWindow)btnGroupFind_Click, End");

			DialogResult = true;
		}
	}
}