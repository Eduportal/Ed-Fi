using System;
using System.Linq;

namespace EdFi.Ods.Common.Utils
{
    public class ConsoleOutput : IOutput
    {
        public void WriteLine(params string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine();
                    break;
                case 1:
                    Console.WriteLine(args[0]);
                    break;
                default:
                    Console.WriteLine(args[0], args.Skip(1).ToArray());
                    break;
            }
        }
    }
}
