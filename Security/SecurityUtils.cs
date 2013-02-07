using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ArcReaction
{
    public static class SecurityUtils
    {
        public static string CreateRandomString(int entropy)
        {
            var bytes = new byte[entropy];

            new RNGCryptoServiceProvider().GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
