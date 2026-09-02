namespace TestRunner.WebApp.Shared.Stores;

public record ChangedAction(
    Version? NewIdeVersion,
    Version? NewRuntimeVersion);