using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SemWork1
{
    internal static class Validator
    {
        public static (bool, string) ValidateEmail(string email)
        {
            var regex = new Regex("/ ^[A - Z0 - 9._ % +-] +@[A - Z0 - 9 -] +.+.[A - Z]{ 2,4}$/ i");
            if (String.IsNullOrEmpty(email))
            {
                return (false, "Email should not be empty");
            }
            if (regex.IsMatch(email))
            {
                return (true, "email is valid");
            }
            return (false, "email is invalid");
        }
    }
}
