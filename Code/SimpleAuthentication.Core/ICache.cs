namespace SimpleAuthentication.Core
{
    public interface ICache
    {
        object this[string key] { get; set; }
        void Initialize();
    }
}