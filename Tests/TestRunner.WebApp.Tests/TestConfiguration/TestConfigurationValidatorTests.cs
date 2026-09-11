namespace TestRunner.WebApp.Tests.TestConfiguration;

using NUnit.Framework;

using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Validation;

[TestFixture]
public class TestConfigurationValidatorTests
{
    private const string AnyRuntimeVersion = "6";

    private TestConfigurationValidator unit;

    [SetUp]
    public void SetUp()
    {
        this.unit = new TestConfigurationValidator();
    }

    [Test]
    public void Validate__WhenEverythingAskedForIsThere__ThenShouldFindNothingWrong()
    {
        // Given:
        TestConfigurationValues values = Values() with
        {
            IsTestLinkEnabled = true,
            IdeVersionText = "6.1.4",
            IsRuntimeTestSelected = true,
            TestedHwAssembly = TestedHwAssemblyType.HW01,
        };

        // When:
        Validity validity = this.unit.Validate(values);

        // Then:
        Assert.That(validity.IsValid, Is.True, validity.Summary);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Validate__WhenNoRuntimeVersionWasChosen__ThenShouldSaySo(string? givenRuntimeVersion)
    {
        // When:
        Validity validity =
            this.unit.Validate(Values() with { RuntimeVersion = givenRuntimeVersion });

        // Then:
        Assert.That(validity.For(nameof(TestConfigurationValues.RuntimeVersion)).Problems, Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenNoRuntimeTestIsSelected__ThenShouldNotAskForATestStation()
    {
        // When:
        Validity validity = this.unit.Validate(
            Values() with { IsRuntimeTestSelected = false, TestedHwAssembly = null });

        // Then:
        Assert.That(validity.IsValid, Is.True, validity.Summary);
    }

    [TestCase(null)]
    [TestCase(TestedHwAssemblyType.Unknown)]
    public void Validate__WhenARuntimeTestIsSelectedWithoutAStation__ThenShouldSaySo(
        TestedHwAssemblyType? givenStation)
    {
        // Given:
        // Unknown stands for a station nobody chose, so it must not pass for one that was.
        TestConfigurationValues values = Values() with
        {
            IsRuntimeTestSelected = true,
            TestedHwAssembly = givenStation,
        };

        // When:
        Validity validity = this.unit.Validate(values);

        // Then:
        Assert.That(validity.For(nameof(TestConfigurationValues.TestedHwAssembly)).Problems, Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenTestLinkIsOff__ThenShouldNotAskForAnIdeVersion()
    {
        // When:
        Validity validity = this.unit.Validate(
            Values() with { IsTestLinkEnabled = false, IdeVersionText = null });

        // Then:
        Assert.That(validity.IsValid, Is.True, validity.Summary);
    }

    [Test]
    public void Validate__WhenTestLinkIsOffAndTheIdeVersionIsNonsense__ThenShouldStillNotMind()
    {
        // Given:
        // A version left over from when TestLink was on is kept rather than thrown away, so it has
        // to be allowed to sit there unused.
        TestConfigurationValues values = Values() with
        {
            IsTestLinkEnabled = false,
            IdeVersionText = "not a version",
        };

        // When:
        Validity validity = this.unit.Validate(values);

        // Then:
        Assert.That(validity.IsValid, Is.True, validity.Summary);
    }

    [TestCase("6.1")]
    [TestCase("6.1.4")]
    [TestCase("10.20.30")]
    public void Validate__WhenTheIdeVersionIsWellFormed__ThenShouldAcceptIt(string givenIdeVersion)
    {
        // When:
        Validity validity = this.unit.Validate(
            Values() with { IsTestLinkEnabled = true, IdeVersionText = givenIdeVersion });

        // Then:
        Assert.That(validity.IsValid, Is.True, validity.Summary);
    }

    [TestCase(null, Description = "nothing typed")]
    [TestCase("", Description = "nothing typed")]
    [TestCase("6", Description = "the minor part is not optional")]
    [TestCase("6.1.4.2", Description = "there is no fourth part")]
    [TestCase("6.1.", Description = "a trailing separator is not a part")]
    [TestCase("v6.1", Description = "digits and dots only")]
    [TestCase("6.1-beta", Description = "no pre-release suffix")]
    [TestCase(" 6.1", Description = "no padding")]
    public void Validate__WhenTheIdeVersionIsNotWellFormed__ThenShouldSaySo(string? givenIdeVersion)
    {
        // When:
        Validity validity = this.unit.Validate(
            Values() with { IsTestLinkEnabled = true, IdeVersionText = givenIdeVersion });

        // Then:
        Assert.That(validity.For(nameof(TestConfigurationValues.IdeVersionText)).Problems, Is.Not.Empty);
    }

    [Test]
    public void Validate__WhenTheIdeVersionIsMissing__ThenShouldSayItOnlyOnce()
    {
        // Given:
        // Empty text is both missing and malformed; saying both would put two messages under one
        // field, of which only the first is any use.
        TestConfigurationValues values = Values() with
        {
            IsTestLinkEnabled = true,
            IdeVersionText = string.Empty,
        };

        // When:
        Validity validity = this.unit.Validate(values);

        // Then:
        Assert.That(
            validity.For(nameof(TestConfigurationValues.IdeVersionText)).Problems,
            Has.Exactly(1).Items);
    }

    [Test]
    public void Validate__WhenSeveralThingsAreWrong__ThenShouldReportAllOfThem()
    {
        // Given:
        TestConfigurationValues values = new(
            RuntimeVersion: null,
            TestedHwAssembly: null,
            IsTestLinkEnabled: true,
            IdeVersionText: null,
            IsRuntimeTestSelected: true);

        // When:
        Validity validity = this.unit.Validate(values);

        // Then:
        Assert.That(validity.Problems, Has.Exactly(3).Items, validity.Summary);
    }

    [Test]
    public void Validate__WhenNothingWasGiven__ThenShouldRefuseRatherThanPassIt()
        => Assert.Throws<ArgumentNullException>(() => this.unit.Validate(null!));

    private static TestConfigurationValues Values()
        => new(
            RuntimeVersion: AnyRuntimeVersion,
            TestedHwAssembly: null,
            IsTestLinkEnabled: false,
            IdeVersionText: null,
            IsRuntimeTestSelected: false);
}
