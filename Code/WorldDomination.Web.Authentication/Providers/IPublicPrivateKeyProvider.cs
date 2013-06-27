namespace WorldDomination.Web.Authentication.Providers
{
    public interface IPublicPrivateKeyProvider
    {
        /// <summary>
        /// Public authorization key.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Private secret key.
        /// </summary>
        /// <remarks>** PLEASE KEEP THIS A SECRET. DON'T COMMIT THIS TO ANY PUBLCI REPOSITORIES. **</remarks>
        string Secret { get; }
    }
}