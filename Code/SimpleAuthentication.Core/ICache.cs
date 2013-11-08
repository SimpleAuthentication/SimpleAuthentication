namespace SimpleAuthentication.Core
{
    public interface ICache
    {
        // NOTE: Not sure if this should be a string (which is what the object will be serialized as).
        string this[string key] { get; set; }
        void Initialize();
    }
}