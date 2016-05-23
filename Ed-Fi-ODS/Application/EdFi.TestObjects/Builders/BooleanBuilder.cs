namespace EdFi.TestObjects.Builders
{
    public class BooleanBuilder : NullableValueBuilderBase<bool>
    {
        private bool nextValue = true;

        protected override bool GetNextValue()
        {
            var value = this.nextValue;
            this.nextValue = !this.nextValue;
            return value;
        }

        public override void Reset()
        {
            this.nextValue = true;
        }
    }
}