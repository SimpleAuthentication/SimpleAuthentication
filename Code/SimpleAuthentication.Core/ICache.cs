namespace SimpleAuthentication.Core
{
    public interface ICache
    {
        CacheData this[string key] { get; set; }
    }
}