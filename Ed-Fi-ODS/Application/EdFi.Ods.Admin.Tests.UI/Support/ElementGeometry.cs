namespace EdFi.Ods.Admin.UITests.Support
{
    public class ElementGeometry
    {
        public bool Exists { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        protected bool Equals(ElementGeometry other)
        {
            return this.Exists.Equals(other.Exists) && this.X == other.X && this.Y == other.Y && this.Height == other.Height && this.Width == other.Width;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ElementGeometry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Exists.GetHashCode();
                hashCode = (hashCode * 397) ^ this.X;
                hashCode = (hashCode * 397) ^ this.Y;
                hashCode = (hashCode * 397) ^ this.Height;
                hashCode = (hashCode * 397) ^ this.Width;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Exists: {0}; X: {1}; Y:{2}; H:{3}; W:{4}", this.Exists, this.X, this.Y, this.Height, this.Width);
        }
    }
}