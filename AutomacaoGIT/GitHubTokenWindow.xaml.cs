using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using AutomacaoGIT.Services.Implementations;

namespace AutomacaoGIT
{
    public partial class GitHubTokenWindow : Window
    {
        private readonly GitHubConfigService _configService;
        public bool TokenWasUpdated { get; private set; }

        public GitHubTokenWindow()
        {
            InitializeComponent();
            _configService = new GitHubConfigService();
            LoadCurrentToken();
        }

        private void LoadCurrentToken()
        {
            var token = _configService.GetToken();
            
            if (!string.IsNullOrEmpty(token))
            {
                TxtToken.Password = token;
                TxtStatus.Text = "? Token configurado";
                TxtStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
                BtnRemove.Visibility = Visibility.Visible;
            }
            else
            {
                TxtStatus.Text = "?? Token não configurado";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Orange;
                BtnRemove.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var token = TxtToken.Password.Trim();

            if (string.IsNullOrEmpty(token))
            {
                System.Windows.MessageBox.Show(
                    "Por favor, insira um token válido.",
                    "Token Inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _configService.SaveToken(token);
            TokenWasUpdated = true;

            System.Windows.MessageBox.Show(
                "Token salvo com sucesso!\n\n" +
                "Agora você pode acessar repositórios privados.",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "Tem certeza que deseja remover o token?\n\n" +
                "Você não poderá acessar repositórios privados.",
                "Confirmar Remoção",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _configService.RemoveToken();
                TokenWasUpdated = true;
                TxtToken.Password = string.Empty;
                LoadCurrentToken();

                System.Windows.MessageBox.Show(
                    "Token removido com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
