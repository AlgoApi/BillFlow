using System.Windows;

namespace BillFlow
{
    public partial class LoginWindow : Window
    {
        DatabaseManager db = new DatabaseManager();
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            db.EnsureAdminExists();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (db.Authenticate(txtUsername.Text, txtPassword.Password))
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
    }
}