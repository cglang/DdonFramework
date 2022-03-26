namespace Ddon.KeyValueStorage
{
    public class DdonKvOptions
    {
        public string StorageName { get; set; } = "ddonStorage";

        public string Directory { get; set; } = "data";

        public bool AutoSave { get; set; } = true;
    }
}
