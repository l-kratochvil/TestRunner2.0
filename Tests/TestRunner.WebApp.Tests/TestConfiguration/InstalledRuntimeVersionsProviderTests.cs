namespace TestRunner.WebApp.Tests.TestConfiguration;

using System.IO;

using Moq;

using NUnit.Framework;

using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores.AppSettings;

[TestFixture]
public class InstalledRuntimeVersionsProviderTests
{
    private const string InstallFolderPath = @"C:\Program Files (x86)\Pertinax6";

    private Mock<IDirectoryReader> directoryReader;
    private Mock<IAppLogger> logger;
    private InstalledRuntimeVersionsProvider unit;

    [SetUp]
    public void SetUp()
    {
        this.directoryReader = new Mock<IDirectoryReader>();
        this.logger = new Mock<IAppLogger>();

        var appSettingsStore = new Mock<IAppSettingsStore>();
        appSettingsStore
            .SetupGet(store => store.Current)
            .Returns(new AppSettingsState(InstallFolderPath));

        var loggerFactory = new Mock<IAppLoggerFactory>();
        loggerFactory
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(this.logger.Object);

        this.unit = new InstalledRuntimeVersionsProvider(
            appSettingsStore.Object,
            this.directoryReader.Object,
            loggerFactory.Object);
    }

    [Test]
    public void Read__WhenTheInstallFolderHoldsVersions__ThenShouldOfferThem()
    {
        // Given:
        this.GivenInstallFolderHolds("6", "7");

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        Assert.That(installed.Versions, Is.EquivalentTo(new[] { "6", "7" }));
    }

    [Test]
    public void Read__WhenTheInstallFolderIsRead__ThenShouldReadTheOneTheSettingsName()
    {
        // Given:
        this.GivenInstallFolderHolds();

        // When:
        this.unit.Read();

        // Then:
        this.directoryReader.Verify(reader => reader.ReadSubFolderNames(InstallFolderPath));
    }

    [Test]
    public void Read__WhenAFolderIsNotAVersion__ThenShouldLeaveItOut()
    {
        // Given:
        // The IDE install folder holds more than the runtimes, so only what starts with a number
        // is one of them — the same rule the console application applies.
        this.GivenInstallFolderHolds("6", "Docs", "_backup", string.Empty);

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        Assert.That(installed.Versions, Is.EqualTo(new[] { "6" }));
    }

    [Test]
    public void Read__WhenSeveralVersionsAreInstalled__ThenShouldOfferTheNewestFirst()
    {
        // Given:
        this.GivenInstallFolderHolds("6", "10", "7");

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        // As numbers rather than as text, so that 10 does not end up between 1 and 2.
        Assert.That(installed.Versions, Is.EqualTo(new[] { "10", "7", "6" }));
    }

    [Test]
    public void Read__WhenAVersionCarriesASuffix__ThenShouldOfferItAboveThePlainOnes()
    {
        // Given:
        // The order the console application established: a folder the tester named for a reason is
        // the one they are most likely after.
        this.GivenInstallFolderHolds("6", "7", "6.1-beta");

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        Assert.That(installed.Versions, Is.EqualTo(new[] { "6.1-beta", "7", "6" }));
    }

    [Test]
    public void Read__WhenNoVersionIsInstalled__ThenShouldSayTheFolderWasReadableAllTheSame()
    {
        // Given:
        // Nothing installed is a different thing from nowhere to look, and the tester deals with
        // each of them differently.
        this.GivenInstallFolderHolds();

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(installed.Versions, Is.Empty);
            Assert.That(installed.IsInstallFolderReadable, Is.True);
        });
    }

    [Test]
    public void Read__WhenTheInstallFolderCannotBeRead__ThenShouldSaySoRatherThanThrow()
    {
        // Given:
        this.GivenInstallFolderCannotBeRead();

        // When:
        InstalledRuntimeVersions installed = this.unit.Read();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(installed.Versions, Is.Empty);
            Assert.That(installed.IsInstallFolderReadable, Is.False);
        });
    }

    [Test]
    public void Read__WhenTheInstallFolderCannotBeRead__ThenShouldTellTheUser()
    {
        // Given:
        this.GivenInstallFolderCannotBeRead();

        // When:
        this.unit.Read();

        // Then:
        this.logger.Verify(
            log => log.Warning(It.Is<string>(message => message.Contains(InstallFolderPath)), It.IsAny<string>()));
    }

    private void GivenInstallFolderHolds(params string[] folderNames)
        => this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(It.IsAny<string>()))
            .Returns(folderNames);

    private void GivenInstallFolderCannotBeRead()
        => this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(It.IsAny<string>()))
            .Throws(new DirectoryNotFoundException());
}