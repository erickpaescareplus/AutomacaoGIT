using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AutomacaoGIT.Services.Implementations
{
    public class GitHubConfigService
    {
        private const string TokenFileName = ".github-token";
        private readonly string _tokenFilePath;

        public GitHubConfigService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "MufasaAutomacoes");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _tokenFilePath = Path.Combine(appFolder, TokenFileName);
        }

        public string? GetToken()
        {
            try
            {
                if (!File.Exists(_tokenFilePath))
                    return null;

                var encryptedData = File.ReadAllBytes(_tokenFilePath);
                var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }

        public void SaveToken(string token)
        {
            try
            {
                var dataToEncrypt = Encoding.UTF8.GetBytes(token);
                var encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_tokenFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar token: {ex.Message}", ex);
            }
        }

        public void RemoveToken()
        {
            try
            {
                if (File.Exists(_tokenFilePath))
                {
                    File.Delete(_tokenFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao remover token: {ex.Message}", ex);
            }
        }
    }
}
