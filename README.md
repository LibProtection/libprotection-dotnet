# LibProtection.NET

## Disclaimer

This project is in the state of early beta. It is stable enough for the public testing, but could be used in a production environment only at your own risk.

**Libprotection.NET** is a .NET version of LibProtection library — an alternative implementation of the standard functionality of the formatted and interpolated strings. It provides a realtime automatic protection from any class of the injection attacks, which belong to the most attacked languages (HTML, URL, JavaScript, SQL and the file paths are currently supported).

| Windows Build Status | Linux Build Status |
|---|---|
|[![Build status](https://ci.appveyor.com/api/projects/status/8y9t294ypeuu18rs/branch/dev?svg=true)](https://ci.appveyor.com/project/libprotection/libprotection-dotnet/branch/dev) | [![Build Status](https://travis-ci.org/LibProtection/libprotection-dotnet.svg?branch=dev)](https://travis-ci.org/LibProtection/libprotection-dotnet)|

## How it works

The library considers each placeholder in a processed interpolated or format string as a potentially injection point. It performs the following actions in each of these points:

1. It decides a grammatical context of the possible injection (taking into account the island grammars, if necessary).
2. If sanitization rules are defined for the given context, then it sanitizes data, which belongs to the placeholder. Otherwise, data inserts as is.
3. It performs tokenization of the input data and counting an amount of tokens. If it exceeds 1, then an attack is reported (by throwing an exception or returning a false value, depending on a used library method).

## Quick example

The following code is vulnerable to injection attacks at three different points (provided that variables a, b and c contain values derived from the input data):

```csharp
Response.Write($"<a href='{a}' onclick='alert("{b}");return false'>{c}</a>");
```

Assume that the attacker passed the following values to the variables a, b and c:

a = ``'onmouseover='alert(`XSS`)``
b = ``");alert(`XSS`)``
c = ``<script>alert(`XSS`)</script>``

After interpolation, the resulting string will look like this:

``<a href=''onmouseover='alert(`XSS`)' onclick='alert("");alert(`XSS`)");return false'><script>alert(`XSS`)</script></a>``

Thus, the attacker has the ability to implement the XSS attack by three different ways. However, after trivial wrapping the interpolated string in the LibProtection API call, this code becomes absolutely protected:

```csharp
Response.Write(SafeString.Format<Html>($"<a href='{a}' onclick='alert("{b}");return false'>{c}</a>"));
```

In this case, after interpolation, the resulting string will look like this:

``<a href='%27onmouseover%3d%27alert(%60XSS%60)' onclick='alert("\&quot;);alert(`XSS`)");return false'>&lt;script&gt;alert(`XSS`)&lt;/script&gt;</a>``

## SafeStringBuilder

:bangbang: This is an experimental feature and should not be used in a production environment.

[SafeStringBuilder](https://github.com/LibProtection/libprotection-dotnet/blob/dev/sources/LibProtection.Injections/LibProtection.Injections/SafeStringBuilder/SafeStringBuilder.cs) class supports mixing tainted and non-tainted substring within one instance. For example, if a substring is added via to an instance of `SafeStringBuilder` via `Append(string value)` method, it will be considered tainted. On the other hand, a substring via `UncheckedAppend(string value)` method, it will be considered safe from being tampered by a potential attacker.
Other methods, like `Insert` or `Replace` also have an "unchecked" version (`UncheckedInsert` and `UncheckedReplace`).

Unlike in `SafeString.Format()` method, attack detection is postponed until a string is built inside an instance of `SafeStringBuilder` and `ToString()` method is called. If an attack is detected, an `AttackDetectedException` will be thrown.

## Try it online

A test site that imitates a vulnerable application protected by the library (only [SafeString.TryFormat](https://github.com/LibProtection/libprotection-dotnet/blob/dev/sources/LibProtection.Injections/LibProtection.Injections/SafeString.cs#L24) for now) is available [here](http://playground.libprotection.org/).

## Additional resources

*"LibProtection: Defeating Injections"* — webinar talk (Russian): [slides](https://speakerdeck.com/kochetkov/libprotection-pobiezhdaia-iniektsii), [video](https://youtu.be/mvFcpnoUfmM).

*"LibProtection: 6 months later"* — meetup talk (Russian): [slides](https://speakerdeck.com/kochetkov/libprotection-6-miesiatsiev-spustia), [video](https://youtu.be/IiHHvE3FdC8?list=PLaKsSq6rTf22r9te6azn43JtMCUlFNtqs).
