using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moo.Core.Utility;
namespace Moo.Core.Security
{
    public static class Captcha
    {
        public class Ticket
        {
            public string Answer;
            public bool GotImage;
        }

        static AutoPopDictionary<int, Ticket> captchas=new AutoPopDictionary<int,Ticket>();
        static char[] CHARACTERS = new char[]{'A','B','D','E','F','G','H','J','N','Q','R','T','Y',
                                              'a','b','d','e','f',    'h',    'n',    'r','t','y',
                                              '2','3','4','5','6','7','8'};
        public static int Generate()
        {
            captchas.AutoPop(DateTime.Now.AddMinutes(-1));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(CHARACTERS[Rand.RAND.Next(CHARACTERS.Length)]);
            }

            int token = Rand.RAND.Next();
            captchas.Add(token, new Ticket() { Answer = sb.ToString(), GotImage = false });
            return token;
        }

        public static bool TokenValid(int token)
        {
            return captchas.ContainsKey(token) && captchas[token] != null;
        }

        public static Ticket GetTicket(int token)
        {
            return captchas[token];
        }

        public static bool Verify(int token, string answer)
        {
            if (!TokenValid(token)) return false;
            string realAnswer = captchas[token].Answer;
            captchas[token] = null;

            return answer.ToLower() == realAnswer.ToLower();
        }
    }
}
