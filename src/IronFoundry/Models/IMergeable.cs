namespace IronFoundry.Types
{
    public interface IMergeable<T>
    {
        void Merge(T obj);
    }
}