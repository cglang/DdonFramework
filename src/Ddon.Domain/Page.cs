namespace Ddon.Domain
{
    public class Page
    {
        public virtual string Sorting { get; set; } = "Id";

        public virtual int Index { get; set; } = 1;

        public virtual int Size { get; set; } = 20;
    }
}
