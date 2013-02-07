using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace ArcReaction
{
    public static class SecureCookieModule
    {
        public static SecureCookie Decrypt(this HttpCookie cookie)
        {
            return new NonEncryptedCookie(cookie);
        }
    }
    
    public abstract class SecureCookie
    {
        protected readonly HttpCookie cookie;

        internal SecureCookie(HttpCookie cookie)
        {
            this.cookie = cookie;
        }

        public string Domain
        {
            get
            {
                return cookie.Domain;
            }
            set
            {
                cookie.Domain = value;
            }
        }

        public string Value
        {
            get
            {
                return cookie.Value;
            }
            set
            {
                cookie.Value = value;
            }
        }

        public System.Collections.Specialized.NameValueCollection Values
        {
            get
            {
                return cookie.Values;
            }
        }

        public DateTime Expires
        {
            get
            {
                return cookie.Expires;
            }
            set
            {
                cookie.Expires = value;
            }
        }

        public bool HasKeys
        {
            get
            {
                return cookie.HasKeys;
            }            
        }

        public bool HttpOnly
        {
            get
            {
                return cookie.HttpOnly;
            }
        }

        public string Name
        {
            get
            {
                return cookie.Name;
            }            
        }

        public bool Sharable
        {
            get
            {
                return cookie.Shareable;
            }
            set
            {
                cookie.Shareable = value;
            }
        }

        public string Path
        {
            get
            {
                return cookie.Path;
            }
            set
            {
                cookie.Path = value;
            }
        }

        public abstract SecureCookie Encrypt();
        public abstract SecureCookie Decrypt();

        #pragma warning disable

        protected static string AttemptDecryption(string s)
        {
            try
            {
                var bytes = Convert.FromBase64String(s);
                var unprotected = MachineKey.Unprotect(bytes, "secure cookie");

                return Encoding.UTF32.GetString(unprotected);
            }
            catch (System.Security.Cryptography.CryptographicException ex) { }
            catch (ArgumentException ex) { }
            catch (FormatException ex) { }

            return s;
        }

        #pragma warning restore
        
        public static implicit operator HttpCookie(SecureCookie cookie)
        {
            return cookie.Encrypt().cookie;
        }
        
        public static implicit operator SecureCookie(HttpCookie cookie)
        {
            var decrypted = AttemptDecryption(cookie.Value);

            return new NonEncryptedCookie(cookie) { Value = decrypted };
        }
    }

    public sealed class EncryptedCookie : SecureCookie
    {
        public EncryptedCookie(string name, string value) : this(new HttpCookie(name, value)) { }        
        
        public EncryptedCookie(HttpCookie cookie)
            : base(cookie)
        {
            var bytes = Encoding.UTF32.GetBytes(cookie.Value ?? "");
            var @protected = MachineKey.Protect(bytes, "secure cookie");

            cookie.Value = Convert.ToBase64String(@protected);
        }
        
        public override SecureCookie Encrypt()
        {
            return this;
        }

        public override SecureCookie Decrypt()
        {
            return new NonEncryptedCookie(cookie);
        }
    }

    public sealed class NonEncryptedCookie : SecureCookie
    {
        public NonEncryptedCookie(HttpCookie cookie)
            : base(cookie)
        {
            cookie.Value = AttemptDecryption(cookie.Value);
        }
        
        public override SecureCookie Decrypt()
        {
            return this;
        }

        public override SecureCookie Encrypt()
        {
            return new EncryptedCookie(cookie);
        }
    }
}
