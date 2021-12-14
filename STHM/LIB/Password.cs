using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STHM.LIB
{
    public static class Password
    {
        private static byte[] key = Encoding.ASCII.GetBytes("sthmsthmsthmsthmsthm123456123456");

        private static byte[] IV = Encoding.ASCII.GetBytes("12345678STHMSTHM");

        private const int CryptoCycle = 3;

        public static string EnPassword(string strPass)
        {
            return Cryptography.Encrypt(strPass, key, IV, CryptoCycle);
        }

        public static string DePassword(string strPass)
        {
            return Cryptography.Decrypt(strPass, key, IV, CryptoCycle);
        }
    }
}
