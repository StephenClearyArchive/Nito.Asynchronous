
namespace Nito.Communication
{
    /// <summary>
    /// This is a special class that may be passed to <c>WriteAsync</c> methods to indicate that <c>WriteCompleted</c> should not be called on success.
    /// </summary>
    public sealed class CallbackOnErrorsOnly { }
}
