using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Control
{
    /// <summary>
    /// UserLineControlMixedGroup.xaml 的交互逻辑
    /// </summary>
    
    public partial class UserLineControlMixedGroup : UserControl
    {
        private Group _group = null;

        public static int ButtonHeight;
        public static RoutedEventHandler ItemButton_Click;
        public static RoutedEventHandler GroupButton_Click;

        public UserLineControlMixedGroup()
        {
            InitializeComponent();
            if(GroupButton_Click != null)
                this.btnGroupTitle.Click += GroupButton_Click;
            this.Loaded += UserLineControlMixedGroup_Loaded;
            this.Unloaded += (sender, args) =>
            {
                if (_group != null)
                {
                    _group.GroupUsers.CollectionChanged -= GroupUsersOnCollectionChanged;
                }
            };
        }

        void UserLineControlMixedGroup_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnGroupTitle.Height = ButtonHeight;
            this.UserLineListBox.Height = grid.Height;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.DataContext != null)
            {
                _group = this.DataContext as Group;
                if (_group != null)
                {
                    _group.GroupUsers.CollectionChanged += GroupUsersOnCollectionChanged;

                    ArrangeUsers(_group);
                }
            }
        }

        private void GroupUsersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            ArrangeUsers(_group);
        }

        private void ArrangeUsers(Group group)
        {
            UserLineListBox.Items.Clear();
            foreach (var user in group.GroupUsers)
            {
                Button button = new Button();
                button.Style = (Style)this.Resources["MixedGroupUserButtonStyle"];
                button.Margin = new Thickness(0);
                button.Height = ButtonHeight;
                button.DataContext = user;
                if (ItemButton_Click != null)
                    button.Click += ItemButton_Click;

                UserLineListBox.Items.Add(button);
            }
        }
    }
}
