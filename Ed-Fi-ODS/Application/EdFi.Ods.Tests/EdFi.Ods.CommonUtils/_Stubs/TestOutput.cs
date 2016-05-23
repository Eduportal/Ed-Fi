namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils._Stubs
{
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.Common.Utils;

    public class TestOutput : IOutput
    {
        private readonly List<string> _output = new List<string>();

        public void WriteLine(params string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    this._output.Add(string.Empty);
                    break;
                case 1:
                    this._output.Add(args[0]);
                    break;
                default:
                    this._output.Add(string.Format(args[0], args.Skip(1).ToArray()));
                    break;
            }
        }

        public string[] AllOutput { get { return this._output.ToArray(); } }
    }
}