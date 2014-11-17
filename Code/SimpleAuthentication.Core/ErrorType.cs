namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Which part of the pipeline the error occured in.
    /// </summary>
    public enum ErrorType
    {
        Unknown,
        RedirectToProvider,
        Callback,
        UserInformation
    }
}