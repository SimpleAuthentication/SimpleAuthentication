namespace Nancy.Authentication.WorldDomination
{
    public class ProcessResult
    {
        public enum ActionEnum
        {
            /// <summary>
            /// To redirect, assign the RedirectTo property on the ProcessResult
            /// </summary>
            Redirect,

            /// <summary>
            /// To Render a view, please supply the View (Name or Path+Name) 
            /// and a ViewModel
            /// </summary>
            RenderView
        }

        public ProcessResult(ActionEnum action)
        {
            Action = action;
        }

        public ActionEnum Action { get; set; }

        /// <summary>
        /// Relative URL to Redirect to (use with Redirect Action)
        /// </summary>
        public string RedirectTo { get; set; }

        /// <summary>
        /// The View (Name or Path+Name) to be rendered (use with RenderView Action)
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// The ViewModel to be rendered with the View (use with RenderView Action)
        /// </summary>
        public dynamic ViewModel { get; set; }
    }
}