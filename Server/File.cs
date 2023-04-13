namespace Server
{
    abstract class File
    {
        public string? FileAnsw { get; set; }
        public string?[]? FileQues { get; set; }
        public int FileMaxNumber { get; set; }

        public enum Kategoria
        {
            A,
            B,
            T
        };

        public abstract void Load(int a, int b, string? c);
    }
}