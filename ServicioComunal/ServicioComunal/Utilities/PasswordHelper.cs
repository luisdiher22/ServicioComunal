using System.Security.Cryptography;
using System.Text;

namespace ServicioComunal.Utilities
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Genera un hash seguro de la contraseña usando SHA256 con salt
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="salt">Salt opcional, si no se proporciona se genera uno automáticamente</param>
        /// <returns>Hash de la contraseña con salt incluido</returns>
        public static string HashPassword(string password, string? salt = null)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Si no se proporciona salt, generar uno
            if (string.IsNullOrEmpty(salt))
            {
                salt = GenerateSalt();
            }

            // Combinar contraseña con salt
            string passwordWithSalt = password + salt;
            
            // Generar hash
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                
                // Convertir a string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                
                // Retornar hash con salt al final (separado por $)
                return builder.ToString() + "$" + salt;
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hash">Hash almacenado en la base de datos</param>
        /// <returns>True si la contraseña es correcta</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Extraer salt del hash
                string[] parts = hash.Split('$');
                if (parts.Length != 2)
                    return false;

                string storedHash = parts[0];
                string salt = parts[1];

                // Generar hash de la contraseña proporcionada con el mismo salt
                string newHash = HashPassword(password, salt);
                string newHashOnly = newHash.Split('$')[0];

                // Comparar hashes
                return storedHash.Equals(newHashOnly, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera un salt aleatorio
        /// </summary>
        /// <returns>Salt en formato string</returns>
        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Genera una contraseña temporal aleatoria
        /// </summary>
        /// <param name="length">Longitud de la contraseña (por defecto 8)</param>
        /// <returns>Contraseña temporal</returns>
        public static string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes);
                
                return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
            }
        }
    }
}
