namespace Nancy.SimpleAuthentication
{
    /// <summary>
    /// The result from the callback Process.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Action types at the end of a callback Process.
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            /// To redirect, assign the RedirectTo property on the ProcessResult.
            /// </summary>
            Redirect,

            /// <summary>
            /// To Render a view, please supply the View (Name or Path+Name) .
            /// and a ViewModel
            /// </summary>
            RenderView
        }

        /// <summary>
        /// Initializes a new instance of the Nancy.SimpleAuthentication.ProcessResult with a specific Action result.
        /// </summary>
        /// <param name="action">The type of action this Process needs to do.</param>
        public ProcessResult(ActionType action)
        {
            Action = action;
        }

        /// <summary>
        /// The type of action this Process needs to do.
        /// </summary>
        public ActionType Action { get; set; }

        /// <summary>
        /// Relative URL to Redirect to (use with Redirect Action).
        /// </summary>
        public string RedirectTo { get; set; }

        /// <summary>
        /// The View (Name or Path+Name) to be rendered (use with RenderView Action).
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// The ViewModel to be rendered with the View (use with RenderView Action).
        /// </summary>
        public dynamic ViewModel { get; set; }
    }
}