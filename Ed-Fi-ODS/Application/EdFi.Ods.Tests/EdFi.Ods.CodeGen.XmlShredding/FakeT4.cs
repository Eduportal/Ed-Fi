namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;

    public class FakeT4
    {
        private string currentIndent = "";
        private readonly List<int> indentLengths = new List<int>();

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(this.currentIndent + format, args);
        }

        public void WriteLine(string textToAppend)
        {
            Console.WriteLine(this.currentIndent + textToAppend);
        }

        public void PushIndent(string indent)
        {
            this.currentIndent = string.Concat(this.currentIndent, indent);
            this.indentLengths.Add(indent.Length);
        }

        public void PopIndent()
        {
            if (this.indentLengths.Count <= 0) return;

            int item = this.indentLengths[this.indentLengths.Count - 1];
            this.indentLengths.RemoveAt(this.indentLengths.Count - 1);
            if (item > 0)
            {
                this.currentIndent = this.currentIndent.Remove(this.currentIndent.Length - item);
            }
        }
    }
}