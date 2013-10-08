namespace SimpleAuthentication.Core
{
    public interface ICache
    {
        void Add(string key, object data);
        object Get(string key);
    }
}