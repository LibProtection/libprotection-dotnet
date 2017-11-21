namespace LibProtection.Injections
{
    internal enum UrlTokenType
    {
        Error,
        Separator,
        Scheme,
        AuthorityEntry,
        PathEntry,
        QueryEntry,
        Fragment,

        // Non-terminals
        SchemeCtx,
        AuthorityCtx,
        PathCtx,
        QueryCtx,
        FragmentCtx
    }
}