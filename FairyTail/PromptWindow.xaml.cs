using System.Windows;

namespace FairyTail
{
    public partial class PromptWindow : Window
    {
        public PromptWindow()
        {
            InitializeComponent();
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
