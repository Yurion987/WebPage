using System.Security.Cryptography;
using System.Text;


namespace daco3.Helpers
{
    public static class Hash
    {
        private const string salt = "šľč1asdľščť5čšg1rš6sdfg4čť7wš6sdf1ščť6gr43gr46ečšťš6+;";

        public static string ZaHashuj(string heslo)
        {

            byte[] hash;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(heslo + salt));
            }
            var sb = new StringBuilder();
            foreach (var item in hash)
            {
                sb.AppendFormat("{0:x2}", item);

            }
            return sb.ToString();

        }
    } 
}
