using System.Security.Cryptography;
using System.Text;

namespace BillFlow
{
    public static class Session
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserRole { get; set; }
        public static bool IsAdmin => CurrentUserRole?.Trim() == "admin";

        // Метод для превращения строки в VARBINARY(64)
        public static byte[] HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}