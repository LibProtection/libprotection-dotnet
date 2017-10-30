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

        // Non-terminals
        AuthorityCtx,
        PathCtx,
        QueryCtx,
    }
}