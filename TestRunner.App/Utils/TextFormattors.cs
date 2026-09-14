public static class TextFormattors
{
    public static string AsErrorText(string text)
        => $"[red]{text.EscapeMarkup()}[/]";

    public static string AsTextValuePair(string text, string? value)
        => value is null
            ? $"[italic]{text.EscapeMarkup()}?[/]"
            : $"{text.EscapeMarkup()}: [yellow]{value.EscapeMarkup()}[/]";
}