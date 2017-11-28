# libprotection-dotnet

**libprotection-dotnet** is a .NET implementation of LibProtection library — an alternative implementation of the standard functionality of formatted and interpolated strings. It provides realtime automatic protection from any class of the injection attacks belongs to the most attacked languages (HTML, URL, JavaScript, SQL and file paths are currently supported).

| Windows Build Status |
|---|
|[![Build status](https://ci.appveyor.com/api/projects/status/d4jggt3p10bvbxik/branch/dev?svg=true)](https://ci.appveyor.com/project/libprotection/libprotection-dotnet/branch/dev)|

## How it works

The library considers each placeholder in a processed format or interpolated string as a potential injection point. In each of these points, it performs the following actions:

1. Decides the grammatical context of the possible injection (taking into account island grammars if necessary).
2. If sanitization rules are defined for the given context, it sanitizes data belonging to the placeholder. Otherwise, data inserts as is.
3. Performs tokenization and counting the number of tokens. If their number exceeds 1, then an attack is reported (by throwing an exception or returning a false value, depending on the formatting method used).

## Quick example

The following code is vulnerable to injection attacks at three different points (provided that variables a, b and c contains values that are sufficiently controlled by the attacker):

```
Response.Write($"<a href='{a}' onclick='alert("{b}");return false'>{c}</a>");
```

Assume that the attacker passed the following values to the variables a, b and c:

a = `'onmouseover='alert(``XSS``)`
b = `");alert(``XSS``)`
c = `<script>alert(``XSS``)</script>`

After interpolation, the resulting string will look like this:

`<a href=''onmouseover='alert(``XSS``)' onclick='alert("");alert(``XSS``)");return false'><script>alert(``XSS``)</script></a>`

Thus, the attacker has the ability to implement the XSS attack in three different ways. However, after trivial wrapping of the interpolated string in the LibProtection API call, this code becomes absolutely protected:

```
Response.Write(SafeString.Format<Html>($"<a href='{a}' onclick='alert("{b}");return false'>{c}</a>"));
```

In this case, after interpolation, the resulting string will look like this:

`<a href='%27onmouseover%3d%27alert(%60XSS%60)' onclick='alert("\&quot;);alert(``XSS``)");return false'>&lt;script&gt;alert(``XSS``)&lt;/script&gt;</a>`
