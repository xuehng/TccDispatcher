using System.Windows;

namespace renstech.NET.SupernovaDispatcher.Control
{
	/// <summary>
	/// MessageWindow.xaml �Ľ����߼�
	/// </summary>
	public partial class MessageWindow : Window
	{
        public enum IconType
        {
            IconQuestion,
            IconError,
            IconWarn,
        }

        public enum ButtonListType
        {
            ButtonOk,
            ButtonOkCancel,
            ButtonContCancel,
            ButtonOkContCancel,
        }

        public enum Result
        {
            ResultNone,
            ResultOk,
            ResultCont,
            ResultCancel,
        }

        public MessageWindow(string title, string content, ButtonListType type, IconType icon = IconType.IconQuestion)
		{
			InitializeComponent();

            DataContext = this;
            
            Title = title;
            
            //��ΪLabel��֧���ı����У�������Label�����TextBlock����֧�ֶ�����ʾ����
            //���Կ��ǽ�Labelȥ��//
            //tbxContent.Content = content;
            txtContent.Text = content;

            ArrangeIcons(icon);
            ArrangeButtons(type);

            App app = Application.Current as App;
            if (app != null && app.AppBkBrush != null)
                Background = app.AppBkBrush;
		}

        public Result WindowResult { get; private set; }

        public bool IsQuestionMark { get; private set; }
        public bool IsWarning { get; private set; }
        public bool IsError { get; private set; }

        private void ArrangeIcons(IconType type)
        {
            IsQuestionMark = false;
            IsError = false;
            IsWarning = false;

            switch (type)
            {
                case IconType.IconQuestion:
                    IsQuestionMark = true;
                    break;
                case IconType.IconError:
                    IsError = true;
                    break;
                case IconType.IconWarn:
                    IsWarning = true;
                    break;
            }            
        }

        private void ArrangeButtons(ButtonListType type)
        {
            switch (type)
            {
                case ButtonListType.ButtonOk:
                    btnOK.Visibility = Visibility.Visible;
                    btnCont.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                    break;
                case ButtonListType.ButtonOkCancel:
                    btnOK.Visibility = Visibility.Visible;
                    btnCont.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
                case ButtonListType.ButtonContCancel:
                    btnOK.Visibility = Visibility.Collapsed;
                    btnCont.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
                case ButtonListType.ButtonOkContCancel:
                    btnOK.Visibility = Visibility.Visible;
                    btnCont.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            WindowResult = Result.ResultOk;
            DialogResult = true;
        }

        private void BtnContClick(object sender, RoutedEventArgs e)
        {
            WindowResult = Result.ResultCont;
            DialogResult = true;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            WindowResult = Result.ResultCancel;
            DialogResult = false;
        }
	}
}