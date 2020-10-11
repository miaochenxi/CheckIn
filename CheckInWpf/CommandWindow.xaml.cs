using System;
using System.Windows;

namespace CheckInWpf
{
    /// <summary>
    /// CommandWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommandWindow : Window
    {
        MainWindow mainWindow;
        public CommandWindow(MainWindow parent)
        {
            InitializeComponent();
            mainWindow = parent;
        }

        private void DoCommand(object sender, RoutedEventArgs e)
        {
            if(command.Text.StartsWith("scmd"))
            {
                switch(command.Text.Substring(5,2))
                {
                    case "-c":SetChecked(command.Text.Substring(8));break;
                }
            }
        }
        
        private void SetChecked(string name)
        {
            foreach (ListItem listitem in mainWindow.list.Items)
            {
                if(listitem.ShowContent.Contains(name))
                {
                    
                    listitem.Checked = true;
                    mainWindow.ID_List.Add(listitem.ID);
                    mainWindow.Check_IN.Add(listitem.ID, DateTime.Now);

                    mainWindow.messages.Text = "签到成功!";
                    mainWindow.notice.Content = "";

                    mainWindow.list.ItemsSource = null;
                    mainWindow.list.ItemsSource = mainWindow.member;
                }
            }
            Close();
        }
    }
}
