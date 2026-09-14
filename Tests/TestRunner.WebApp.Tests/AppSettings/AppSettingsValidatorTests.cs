namespace TestRunner.WebApp.Tests.AppSettings;

using System.IO;

using Moq;
using NUnit.Framework;

using TestRunner.WebApp.Features.AppSettings.Services;
using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// What an IDE install folder has to be before the settings naming it can be saved.
/// </summary>
[TestFixture]
public class AppSettingsValidatorTests
{
    private const string AnyFolderPath = @"C:\Ide";

    private Mock<IDirectoryReader> directoryReader;
    private AppSettingsValidator unit;

    [SetUp]
    public void SetUp()
    {
        this.directoryReader = new Mock<IDirectoryReader>();
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(It.IsAny<string>()))
            .Returns<string>(static _ => ["6"]);

        this.unit = new AppSettingsValidator(this.directoryReader.Object);
    }

    [Test]
    public void Validate__WhenTheInstallFolderHoldsRuntimeVersions__ThenShouldFindNothingWrong()
    {
        // When:
        Validity validity = this.unit.Validate(new AppSettingsSource(AnyFolderPath));

        // Then:
        Assert.That(validity.Problems, Is.Empty, validity.Summary);
    }

    [Test]
    public void Validate__WhenTheInstallFolderCannotBeRead__ThenShouldRefuseIt()
    {
        // Given:
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(AnyFolderPath))
            .Throws(new DirectoryNotFoundException());

        // When:
        Validity validity = this.unit.Validate(new AppSettingsSource(AnyFolderPath));

        // Then:
        Assert.That(validity.IsValid, Is.False, validity.Summary);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Validate__WhenNoInstallFolderWasEntered__ThenShouldRefuseIt(string givenFolderPath)
    {
        // When:
        Validity validity = this.unit.Validate(new AppSettingsSource(givenFolderPath));

        // Then:
        Assert.That(validity.IsValid, Is.False, validity.Summary);
    }

    [Test]
    public void Validate__WhenTheInstallFolderHoldsNoRuntimeVersion__ThenShouldSaySoWithoutRefusingIt()
    {
        // Given:
        // The folder is there, so it is the tester's installation that is wanting and not the path
        // they typed.
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(AnyFolderPath))
            .Returns<string>(static _ => []);

        // When:
        Validity validity = this.unit.Validate(new AppSettingsSource(AnyFolderPath));

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(validity.IsValid, Is.True, validity.Summary);
            Assert.That(validity.Problems, Has.Exactly(1).Items);
        });
    }

    [Test]
    public void Validate__WhenSomethingIsWrong__ThenShouldKeyItToTheInstallFolder()
    {
        // Given:
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(AnyFolderPath))
            .Throws(new IOException());

        // When:
        Validity validity = this.unit.Validate(new AppSettingsSource(AnyFolderPath));

        // Then:
        Assert.That(
            validity.For(nameof(IAppSettingsValidationSource.IdeInstallFolderPath)).Problems,
            Has.Exactly(1).Items);
    }

    private sealed record AppSettingsSource(string IdeInstallFolderPath)
        : IAppSettingsValidationSource;
}
