namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    using System.Collections.Generic;

    public class FKTable
    {
        public string ThisTableSchema { get; set; }
        public string ThisTable { get; set; }
        public List<string> ThisColumns { get; set; }
        public string OtherTableSchema{ get; set; }
        public string OtherTable{ get; set; }
        public List<string> OtherColumns { get; set; }
        public string OtherClass{ get; set; }
        public string OtherQueryable{ get; set; }
        public bool IsPrimaryTable{ get; set; }
        public string ConstraintName{ get; set; }

        public FKTable()
        {
           this.ThisColumns = new List<string>();
           this.OtherColumns = new List<string>();
        }

        public override string ToString()
        {
            return string.Format(this.IsPrimaryTable ? "{0}.{1}.{2} -> {3}.{4}.{5}" : "{0}.{1}.{2} <- {3}.{4}.{5}",
                this.ThisTableSchema,
                this.ThisTable,
                string.Join("|", this.ThisColumns),
                this.OtherTableSchema,
                this.OtherTable,
                string.Join("|", this.OtherColumns));
        }
    }
}