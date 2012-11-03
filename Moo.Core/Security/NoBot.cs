using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Moo.Core.Security
{
    public static class NoBot
    {
        [ThreadStatic]
        static bool Verified;

        public static void Verrify(int token,string answer)
        {
            Verified = Captcha.Verify(token, answer);
        }

        public static void Require()
        {
            if (!Verified) throw new NeedCaptchaException();
        }


    }

    public class NeedCaptchaException : Exception
    {}
}
