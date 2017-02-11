using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackerCmd
{
    public class ArgumentHelper
    {
        public static bool Has(string[] Args, List<string> Names)
        {
            foreach (var name in Names)
            {
                if (Has(Args, name))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Has(string[] Args, string Name)
        {
            var NameLower = Name.ToLower();

            foreach (var Arg in Args)
            {
                var ArgLower = Arg.ToLower();

                if (ArgLower.Equals(NameLower) || ArgLower.StartsWith(String.Format("{0}=", NameLower)))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetValue(string[] Args, List<string> Names)
        {
            foreach (var name in Names)
            {
                var ret = GetValue(Args, name);
                if (!String.IsNullOrEmpty(ret))
                {
                    return ret;
                }
            }

            return String.Empty;
        }

        public static string GetValue(string[] Args, string Name)
        {
            foreach (var arg in Args)
            {
                if (arg.ToLower().StartsWith(String.Format("{0}=", Name.ToLower())))
                {
                    var index = arg.IndexOf('=');

                    if (index > 0)
                    {
                        return arg.Substring(index + 1, arg.Length - index - 1);
                    }
                }
            }

            return String.Empty;
        }

        public static int GetValueInt(string[] Args, string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return 0;
            }

            var ValueString = GetValue(Args, Name);

            int Ret = 0;

            try
            {
                int.TryParse(ValueString, out Ret);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ret;
        }

        public static bool GetValueBoolean(string[] Args, string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return false;
            }

            var ValueString = GetValue(Args, Name);

            if (ValueString.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if (ValueString.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            int Ret = 0;

            try
            {
                int.TryParse(ValueString, out Ret);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (Ret > 0);
        }
    }
}
