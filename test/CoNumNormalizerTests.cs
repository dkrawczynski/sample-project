using System;
using NUnit.Framework;

namespace LSC_CofRoHSReport.Tests;

// Mirrors the CoNum pre-processing logic from LSC_CofRoHSReportExtensionClass.
// Extracted here because that class couples the logic to the IDO runtime (Context.Commands).
internal static class CoNumNormalizer
{
    internal const int CoNumTypeLength = 10;

    internal static string Normalize(string? coNum)
    {
        if (!string.IsNullOrEmpty(coNum) &&
            !coNum.StartsWith("S", StringComparison.OrdinalIgnoreCase))
        {
            coNum = "S" + coNum;
        }
        return (coNum ?? string.Empty).PadRight(CoNumTypeLength).Substring(0, CoNumTypeLength);
    }
}

public class CoNumNormalizerTests
{
    [Test]
    public void Normalize_WhenCoNumLacksSPrefix_PrependsSPrefix()
    {
        string result = CoNumNormalizer.Normalize("0000001");
        Assert.That(result, Is.EqualTo("S0000001  "));
    }

    [Test]
    public void Normalize_WhenCoNumAlreadyHasUppercaseSPrefix_DoesNotDoublePrefix()
    {
        string result = CoNumNormalizer.Normalize("S0000001");
        Assert.That(result, Is.EqualTo("S0000001  "));
    }

    [Test]
    public void Normalize_WhenCoNumHasLowercaseSPrefix_DoesNotDoublePrefix()
    {
        string result = CoNumNormalizer.Normalize("s0000001");
        Assert.That(result, Is.EqualTo("s0000001  "));
    }

    [Test]
    public void Normalize_ResultIsPaddedToExactlyTenCharacters()
    {
        string result = CoNumNormalizer.Normalize("S001");
        Assert.That(result.Length, Is.EqualTo(CoNumNormalizer.CoNumTypeLength));
    }

    [Test]
    public void Normalize_WhenCoNumIsNull_ReturnsBlankPaddedToTenCharacters()
    {
        string result = CoNumNormalizer.Normalize(null);
        Assert.That(result, Is.EqualTo(new string(' ', CoNumNormalizer.CoNumTypeLength)));
    }

    [Test]
    public void Normalize_WhenCoNumIsEmpty_ReturnsBlankPaddedToTenCharacters()
    {
        string result = CoNumNormalizer.Normalize(string.Empty);
        Assert.That(result, Is.EqualTo(new string(' ', CoNumNormalizer.CoNumTypeLength)));
    }

    [Test]
    public void Normalize_WhenInputAlreadyTenChars_DoesNotTruncate()
    {
        string result = CoNumNormalizer.Normalize("S000000001");
        Assert.That(result, Is.EqualTo("S000000001"));
        Assert.That(result.Length, Is.EqualTo(CoNumNormalizer.CoNumTypeLength));
    }

    [Test]
    public void Normalize_PaddingCharacterIsSpace()
    {
        string result = CoNumNormalizer.Normalize("S1");
        Assert.That(result, Is.EqualTo("S1".PadRight(CoNumNormalizer.CoNumTypeLength)));
    }

    [TestCase("0000001",    "S0000001  ", TestName = "AddsSAndPads")]
    [TestCase("S0000001",   "S0000001  ", TestName = "KeepsExistingSAndPads")]
    [TestCase("s0000001",   "s0000001  ", TestName = "KeepsLowercaseSAndPads")]
    [TestCase("000000001",  "S000000001", TestName = "NineDigitsGetsSAndFitsExactly")]
    [TestCase("S000000001", "S000000001", TestName = "TenCharsWithSUnchanged")]
    [TestCase("0000000001", "S000000000", TestName = "TenDigitsGetsSAndTruncatesToTen")]
    public void Normalize_ProducesExpectedOutput(string input, string expected)
    {
        Assert.That(CoNumNormalizer.Normalize(input), Is.EqualTo(expected));
    }
}
