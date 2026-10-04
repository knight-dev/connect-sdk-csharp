// `init`-only setters and positional records need System.Runtime.CompilerServices.IsExternalInit,
// which net5+ ships but netstandard2.1 does not. The official workaround is to
// declare it ourselves in any namespace (the compiler matches by full name).
// Guarded with #if so net8 doesn't get a duplicate definition.

#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif
