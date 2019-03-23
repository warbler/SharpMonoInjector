using System.Globalization;
using System.Linq;

namespace SharpMonoInjector.Console
{
    public class CommandLineArguments
    {
        private readonly string[] _args;

        public CommandLineArguments(string[] args)
        {
            _args = args;
        }

        public bool IsSwitchPresent(string name) => _args.Any(arg => arg == name);

        public bool GetLongArg(string name, out long value)
        {
            if (GetStringArg(name, out string str))
                return long.TryParse(str.StartsWith("0x") ? str.Substring(2) : str, NumberStyles.AllowHexSpecifier, null, out value);

            value = default(long);
            return false;
        }

        public bool GetIntArg(string name, out int value)
        {
            if (GetStringArg(name, out string str))
                return int.TryParse(str.StartsWith("0x") ? str.Substring(2) : str, NumberStyles.AllowHexSpecifier, null, out value);

            value = default(int);
            return false;
        }

        public bool GetStringArg(string name, out string value)
        {
            for (int i = 0; i < _args.Length; i++) {
                string arg = _args[i];

                if (arg == name) {
                    if (i == _args.Length - 1)
                        break;

                    value = _args[i + 1];
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
