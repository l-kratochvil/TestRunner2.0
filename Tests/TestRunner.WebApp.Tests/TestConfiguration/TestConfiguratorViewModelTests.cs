namespace TestRunner.WebApp.Tests.TestConfiguration;

using NUnit.Framework;

using TestRunner.WebApp.Features.TestConfiguration.Components;
using TestRunner.WebApp.Features.TestConfiguration.Models;
using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Stores.TestConfiguration;
using TestRunner.WebApp.Shared.Validation;

[TestFixture]
public class TestConfiguratorViewModelTests
{
    private TestConfiguratorViewModel unit;

    [SetUp]
    public void SetUp()
    {
        // The real rules rather than a mock: what the view model shows is the rules applied to what
        // is being typed, so a stand-in would leave the interesting part untested.
        this.unit = new TestConfiguratorViewModel(new TestConfigurationValidator());
    }

    [Test]
    public void Load__WhenAConfigurationIsOpened__ThenShouldShowWhatItHolds()
    {
        // Given:
        TestConfigurationState state = new(
            IsTestLinkEnabled: true,
            IdeVersion: new Version(6, 1),
            RuntimeVersion: "6",
            TestedHwAssembly: TestedHwAssemblyType.HW01);

        // When:
        this.unit.Load(state, isRuntimeTestSelected: true);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(this.unit.RuntimeVersion, Is.EqualTo("6"));
            Assert.That(this.unit.TestedHwAssembly, Is.EqualTo(TestedHwAssemblyType.HW01));
            Assert.That(this.unit.IsTestLinkEnabled, Is.True);
            Assert.That(this.unit.IdeVersionText, Is.EqualTo("6.1"));
        });
    }

    [Test]
    public void ErrorFor__WhenAConfigurationHasJustBeenOpened__ThenShouldSayNothingAboutItYet()
    {
        // Given:
        // An empty configuration nobody has touched is not a mistake anyone made yet.
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(
                this.unit.ErrorFor(nameof(TestConfigurationValues.RuntimeVersion)),
                Is.Null);
            Assert.That(this.unit.Validity.IsValid, Is.False);
        });
    }

    [Test]
    public void ErrorFor__WhenTheTesterHasBeenToAField__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetRuntimeVersion(null);

        // Then:
        Assert.That(
            this.unit.ErrorFor(nameof(TestConfigurationValues.RuntimeVersion)),
            Is.Not.Null);
    }

    [Test]
    public void ErrorFor__WhenAnotherFieldWasTouched__ThenShouldStillSayNothingAboutThisOne()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: true);

        // When:
        this.unit.SetRuntimeVersion("6");

        // Then:
        Assert.That(
            this.unit.ErrorFor(nameof(TestConfigurationValues.TestedHwAssembly)),
            Is.Null);
    }

    [Test]
    public void SetRuntimeVersion__WhenTheChoiceIsCleared__ThenShouldHoldNothingRatherThanEmptyText()
    {
        // Given:
        // The empty choice of the combo box arrives as an empty string, which is not a version
        // named after nothing.
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetRuntimeVersion(string.Empty);

        // Then:
        Assert.That(this.unit.RuntimeVersion, Is.Null);
    }

    [Test]
    public void IsTestedHwAssemblyShown__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForAStation()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // Then:
        Assert.That(this.unit.IsTestedHwAssemblyShown, Is.False);
    }

    [Test]
    public void SetRuntimeTestSelected__WhenARuntimeTestIsPicked__ThenShouldStartAskingForAStation()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetRuntimeTestSelected(true);

        // Then:
        Assert.That(this.unit.IsTestedHwAssemblyShown, Is.True);
    }

    [Test]
    public void SetRuntimeTestSelected__WhenARuntimeTestIsPicked__ThenShouldMindTheMissingStation()
    {
        // Given:
        this.unit.Load(
            new TestConfigurationState() with { RuntimeVersion = "6" },
            isRuntimeTestSelected: false);

        // When:
        this.unit.SetRuntimeTestSelected(true);

        // Then:
        Assert.That(this.unit.Validity.IsValid, Is.False);
    }

    [Test]
    public void IsIdeVersionShown__WhenTestLinkIsTurnedOn__ThenShouldStartAskingForTheIdeVersion()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetTestLinkEnabled(true);

        // Then:
        Assert.That(this.unit.IsIdeVersionShown, Is.True);
    }

    [Test]
    public void SetTestLinkEnabled__WhenItIsTurnedOffAgain__ThenShouldKeepWhatWasTyped()
    {
        // Given:
        // Ticking a box off must not throw away work; the version is simply not asked about.
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);
        this.unit.SetTestLinkEnabled(true);
        this.unit.SetIdeVersionText("6.1");

        // When:
        this.unit.SetTestLinkEnabled(false);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(this.unit.IdeVersionText, Is.EqualTo("6.1"));
            Assert.That(this.unit.IsIdeVersionShown, Is.False);
        });
    }

    [Test]
    public void IdeVersion__WhenTheTextIsAVersion__ThenShouldHandItOverToBeKept()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetIdeVersionText("6.1.4");

        // Then:
        Assert.That(this.unit.IdeVersion, Is.EqualTo(new Version(6, 1, 4)));
    }

    [Test]
    public void IdeVersion__WhileTheTextIsBeingTyped__ThenShouldHoldNothingToKeepYet()
    {
        // Given:
        this.unit.Load(new TestConfigurationState(), isRuntimeTestSelected: false);

        // When:
        this.unit.SetIdeVersionText("6.");

        // Then:
        Assert.That(this.unit.IdeVersion, Is.Null);
    }

    [Test]
    public void RuntimeVersionsNote__WhenVersionsAreInstalled__ThenShouldHaveNothingToSay()
    {
        // Given:
        this.unit.SetInstalledRuntimeVersions(
            new InstalledRuntimeVersions(["6"], IsInstallFolderReadable: true));

        // Then:
        Assert.That(this.unit.RuntimeVersionsNote, Is.Null);
    }

    [Test]
    public void RuntimeVersionsNote__WhenNoneIsInstalled__ThenShouldSayThereIsNothingToChooseFrom()
    {
        // Given:
        this.unit.SetInstalledRuntimeVersions(
            new InstalledRuntimeVersions([], IsInstallFolderReadable: true));

        // Then:
        Assert.That(this.unit.RuntimeVersionsNote, Does.Contain("installed"));
    }

    [Test]
    public void RuntimeVersionsNote__WhenTheInstallFolderCannotBeRead__ThenShouldPointAtTheSettings()
    {
        // Given:
        // Nowhere to look is the tester's to fix in the settings, and is a different thing from an
        // install folder holding nothing yet.
        this.unit.SetInstalledRuntimeVersions(
            new InstalledRuntimeVersions([], IsInstallFolderReadable: false));

        // Then:
        Assert.That(this.unit.RuntimeVersionsNote, Does.Contain("settings"));
    }

    [Test]
    public void Load__WhenNothingWasGiven__ThenShouldRefuseRatherThanEditNothing()
        => Assert.Throws<ArgumentNullException>(
            () => this.unit.Load(null!, isRuntimeTestSelected: false));
}
