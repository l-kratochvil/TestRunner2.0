namespace TestRunner.WebApp.Features.AppSettings.Services;

using FluentValidation;

using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// The rules the application settings have to meet before they can be saved.
/// </summary>
/// <remarks>
/// Written with FluentValidation but kept behind <see cref="IAppSettingsValidator"/>, as the test
/// configuration rules are. An install folder is only worth anything if the test machine can read
/// it, so the rules go to the filesystem through <see cref="IDirectoryReader"/> rather than guess
/// from the text.
/// </remarks>
/// <param name="directoryReader">Reads what the install folder contains.</param>
public sealed class AppSettingsValidator(IDirectoryReader directoryReader) : IAppSettingsValidator
{
    private readonly Rules rules = new(directoryReader);

    /// <inheritdoc/>
    public Validity Validate(IAppSettingsValidationSource source)
    {
        return this.rules.Validate(source).ToValidity();
    }

    /// <remarks>
    /// Nested and private, so that FluentValidation stays an implementation detail of the rules.
    /// The failures it produces are keyed by the property names of
    /// <see cref="IAppSettingsValidationSource"/>, which is what the editor looks its messages up
    /// by.
    /// </remarks>
    private sealed class Rules : AbstractValidator<IAppSettingsValidationSource>
    {
        public Rules(IDirectoryReader directoryReader)
        {
            // A folder the test machine cannot read leaves the application pointed at a place it
            // will never find a runtime version in, so it is refused rather than noted. Holding no
            // runtime version yet is the tester's own installation and only worth telling them
            // about, which is why it does not stop the settings being saved.
            this.RuleFor(static source => source.IdeInstallFolderPath)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Enter the folder the IDE is installed in.")
                .Must(path => ReadSubFolderNames(directoryReader, path) is not null)
                .WithMessage("This folder does not exist, or cannot be reached from the test machine.")
                .Must(path => ReadSubFolderNames(directoryReader, path) is not { Count: 0 })
                .WithSeverity(FluentValidation.Severity.Warning)
                .WithMessage("No runtime version is installed in this folder.");
        }

        /// <returns>
        /// What the folder contains, or <see langword="null"/> when it could not be read.
        /// </returns>
        private static IReadOnlyList<string>? ReadSubFolderNames(
            IDirectoryReader directoryReader, string path)
        {
            try
            {
                return directoryReader.ReadSubFolderNames(path);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return null;
            }
        }
    }
}