namespace LibProtection.Injections
{
    internal enum UrlTokenType
    {
        Error,
        Scheme,
        AuthorityEntry,
        PathEntry,
        QueryEntry,
        Fragment,
        Separator,

        // Non-terminals
        SchemeCtx,
        AuthorityCtx,
        PathCtx,
        QueryCtx,
        FragmentCtx
    }
}