namespace TestRunner.App.Common;

using CSharpFunctionalExtensions;

internal class Choice<TValue>(
    TValue value,
    string displayText,
    string? displayValue = null)
{
    public string Text { get; } = TextFormattors.AsTextValuePair(displayText, displayValue);

    public TValue Value { get; } = value;
}

internal static class Choice
{
    public static Maybe<Choice<TChoiceValue>> InitChoice<TChoiceValue>(
        TChoiceValue choiceValue,
        string choiceDisplayText,
        string? choiceDisplayValue,
        Func<bool>? shouldInitPredicate = null)
        => shouldInitPredicate?.Invoke() ?? true
            ? Maybe<Choice<TChoiceValue>>.From(new Choice<TChoiceValue>(
                value: choiceValue,
                displayText: choiceDisplayText,
                displayValue: choiceDisplayValue))
            : Maybe<Choice<TChoiceValue>>.None;
}