namespace System.Collections.Concurrent
{
    public interface IEditableCollection
    {
        void BeginEditingItem();
        void EndedEditingItem();
    }
}
