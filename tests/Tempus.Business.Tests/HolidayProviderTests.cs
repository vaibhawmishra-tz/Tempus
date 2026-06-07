using Microsoft.Extensions.DependencyInjection;
using Tempus.Business.Abstractions;
using Tempus.Holidays;
using Tempus.Holidays.Exchange;
using Tempus.Holidays.EU;
using Tempus.Holidays.India;
using Tempus.Holidays.UK;
using Tempus.Holidays.US;

namespace Tempus.Business.Tests;

public class HolidayProviderTests
{
    // US

    [Fact]
    public void UsHolidays_NewYearsDay2026_IsHoliday()
    {
        // Jan 1 2026 is Thursday — no shift
        new UsHolidayProvider().IsHoliday(new DateOnly(2026, 1, 1)).Should().BeTrue();
    }

    [Fact]
    public void UsHolidays_IndependenceDay2026_ObservedFriday()
    {
        // Jul 4 2026 is Saturday → observed Jul 3 (Friday)
        new UsHolidayProvider().IsHoliday(new DateOnly(2026, 7, 3)).Should().BeTrue();
    }

    [Fact]
    public void UsHolidays_Christmas2026_IsHoliday()
    {
        // Dec 25 2026 is Friday — no shift
        new UsHolidayProvider().IsHoliday(new DateOnly(2026, 12, 25)).Should().BeTrue();
    }

    [Fact]
    public void UsHolidays_Region_IsUS()
    {
        new UsHolidayProvider().Region.Should().Be("US");
    }

    [Fact]
    public void UsHolidays_2026_ContainsTenOrMoreHolidays()
    {
        new UsHolidayProvider().GetHolidays(2026).Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void UsHolidays_Juneteenth_NotPresentBefore2021()
    {
        var pre = new UsHolidayProvider().GetHolidays(2020).Select(h => h.Name);
        pre.Should().NotContain(n => n.Contains("Juneteenth"));
    }

    [Fact]
    public void UsHolidays_Juneteenth_PresentFrom2021()
    {
        var post = new UsHolidayProvider().GetHolidays(2021).Select(h => h.Name);
        post.Should().Contain(n => n.Contains("Juneteenth"));
    }

    // UK

    [Fact]
    public void UkHolidays_Region_IsUK()
    {
        new UkHolidayProvider().Region.Should().Be("UK");
    }

    [Fact]
    public void UkHolidays_2026_ContainsEightHolidays()
    {
        new UkHolidayProvider().GetHolidays(2026).Should().HaveCount(8);
    }

    [Fact]
    public void UkHolidays_IsHoliday_ChristmasDay2026()
    {
        new UkHolidayProvider().IsHoliday(new DateOnly(2026, 12, 25)).Should().BeTrue();
    }

    [Fact]
    public void UkHolidays_GoodFriday_ExistsIn2026()
    {
        var names = new UkHolidayProvider().GetHolidays(2026).Select(h => h.Name);
        names.Should().Contain("Good Friday");
    }

    // India

    [Fact]
    public void InHolidays_Region_IsIN()
    {
        new InHolidayProvider().Region.Should().Be("IN");
    }

    [Fact]
    public void InHolidays_IndependenceDay_August15()
    {
        new InHolidayProvider().IsHoliday(new DateOnly(2026, 8, 15)).Should().BeTrue();
    }

    [Fact]
    public void InHolidays_RepublicDay_January26()
    {
        new InHolidayProvider().IsHoliday(new DateOnly(2026, 1, 26)).Should().BeTrue();
    }

    [Fact]
    public void InHolidays_RandomWeekday_IsFalse()
    {
        new InHolidayProvider().IsHoliday(new DateOnly(2026, 3, 15)).Should().BeFalse();
    }

    // EU

    [Fact]
    public void EuHolidays_Region_IsEU()
    {
        new EuHolidayProvider().Region.Should().Be("EU");
    }

    [Fact]
    public void EuHolidays_NewYearsDay_January1()
    {
        new EuHolidayProvider().IsHoliday(new DateOnly(2026, 1, 1)).Should().BeTrue();
    }

    [Fact]
    public void EuHolidays_LabourDay_May1()
    {
        new EuHolidayProvider().IsHoliday(new DateOnly(2026, 5, 1)).Should().BeTrue();
    }

    [Fact]
    public void EuHolidays_2026_ContainsEightHolidays()
    {
        new EuHolidayProvider().GetHolidays(2026).Should().HaveCount(8);
    }

    // NYSE

    [Fact]
    public void NyseHolidays_Region_IsNYSE()
    {
        new NyseHolidayProvider().Region.Should().Be("NYSE");
    }

    [Fact]
    public void NyseHolidays_GoodFriday_ExistsIn2026()
    {
        var names = new NyseHolidayProvider().GetHolidays(2026).Select(h => h.Name);
        names.Should().Contain("Good Friday");
    }

    [Fact]
    public void NyseHolidays_IndependenceDay_IsObservedFriday_In2026()
    {
        // Jul 4 2026 is Saturday → observed Friday Jul 3
        new NyseHolidayProvider().IsHoliday(new DateOnly(2026, 7, 3)).Should().BeTrue();
    }

    [Fact]
    public void NyseHolidays_Juneteenth_PresentFrom2021()
    {
        var names = new NyseHolidayProvider().GetHolidays(2022).Select(h => h.Name);
        names.Should().Contain(n => n.Contains("Juneteenth"));
    }

    // IsHoliday caching

    [Fact]
    public void IsHoliday_CalledTwice_ReturnsSameResult()
    {
        IHolidayProvider provider = new UsHolidayProvider();
        var date = new DateOnly(2026, 1, 1);
        bool first = provider.IsHoliday(date);
        bool second = provider.IsHoliday(date);
        first.Should().Be(second);
    }

    // ── Tempus.Holidays (data-driven) ─────────────────────────────────────────

    private static IHolidayProvider LoadProvider(string region)
    {
        var services = new ServiceCollection();
        services.AddHolidays(region);
        return services.BuildServiceProvider()
            .GetServices<IHolidayProvider>()
            .First(p => string.Equals(p.Region, region, StringComparison.Ordinal));
    }

    // CA — Canada Federal

    [Fact]
    public void CaHolidays_Region_IsCA()
        => LoadProvider("CA").Region.Should().Be("CA");

    [Fact]
    public void CaHolidays_NewYearsDay2026_IsHoliday()
        => LoadProvider("CA").IsHoliday(new DateOnly(2026, 1, 1)).Should().BeTrue();

    [Fact]
    public void CaHolidays_VictoriaDay2026_IsMay25()
    {
        // May 25 2026 is Monday — falls exactly on the boundary, so Victoria Day = May 25
        LoadProvider("CA").GetHolidays(2026)
            .Should().Contain(h => h.Name == "Victoria Day" && h.Date == new DateOnly(2026, 5, 25));
    }

    [Fact]
    public void CaHolidays_Ndttr_NotPresentBefore2021()
    {
        LoadProvider("CA").GetHolidays(2020)
            .Should().NotContain(h => h.Name.Contains("Truth"));
    }

    [Fact]
    public void CaHolidays_Ndttr_PresentFrom2021()
    {
        LoadProvider("CA").GetHolidays(2021)
            .Should().Contain(h => h.Name.Contains("Truth"));
    }

    [Fact]
    public void CaHolidays_BoxingDay2026_ObservedMonday()
    {
        // Christmas 2026 = Fri Dec 25. Boxing Day Dec 26 = Sat → observed Mon Dec 28.
        LoadProvider("CA").IsHoliday(new DateOnly(2026, 12, 28)).Should().BeTrue();
    }

    // CA-ON — Ontario

    [Fact]
    public void CaOnHolidays_Region_IsCaOn()
        => LoadProvider("CA-ON").Region.Should().Be("CA-ON");

    [Fact]
    public void CaOnHolidays_FamilyDay2026_IsFeb16()
    {
        // Feb 1 2026 = Sun → 1st Mon = Feb 2, 2nd = Feb 9, 3rd = Feb 16
        LoadProvider("CA-ON").IsHoliday(new DateOnly(2026, 2, 16)).Should().BeTrue();
    }

    [Fact]
    public void CaOnHolidays_NoRemembranceDay()
    {
        LoadProvider("CA-ON").GetHolidays(2026)
            .Should().NotContain(h => h.Name.Contains("Remembrance"));
    }

    // CA-QC — Quebec

    [Fact]
    public void CaQcHolidays_Region_IsCaQc()
        => LoadProvider("CA-QC").Region.Should().Be("CA-QC");

    [Fact]
    public void CaQcHolidays_FeteNationale2026_IsJun24()
        => LoadProvider("CA-QC").IsHoliday(new DateOnly(2026, 6, 24)).Should().BeTrue();

    [Fact]
    public void CaQcHolidays_PatriotsDay2026_IsMay25()
    {
        // Same date computation as Victoria Day — last Monday on or before May 25
        LoadProvider("CA-QC").GetHolidays(2026)
            .Should().Contain(h => h.Name.Contains("Patriots") && h.Date == new DateOnly(2026, 5, 25));
    }

    // AU — Australia National

    [Fact]
    public void AuHolidays_Region_IsAU()
        => LoadProvider("AU").Region.Should().Be("AU");

    [Fact]
    public void AuHolidays_AustraliaDay2026_IsHoliday()
        => LoadProvider("AU").IsHoliday(new DateOnly(2026, 1, 26)).Should().BeTrue();

    [Fact]
    public void AuHolidays_AnzacDay2026_ObservedMonday()
    {
        // Apr 25 2026 = Saturday → observed Mon Apr 27
        LoadProvider("AU").IsHoliday(new DateOnly(2026, 4, 27)).Should().BeTrue();
    }

    [Fact]
    public void AuHolidays_2026_ContainsEightHolidays()
        => LoadProvider("AU").GetHolidays(2026).Should().HaveCount(8);

    [Fact]
    public void AuHolidays_BoxingDay2026_ObservedMonday()
    {
        // Christmas 2026 Fri → Boxing Dec 26 Sat → observed Mon Dec 28
        LoadProvider("AU").IsHoliday(new DateOnly(2026, 12, 28)).Should().BeTrue();
    }

    // DE — Germany Federal

    [Fact]
    public void DeHolidays_Region_IsDE()
        => LoadProvider("DE").Region.Should().Be("DE");

    [Fact]
    public void DeHolidays_GoodFriday2026_IsApril3()
    {
        // Easter 2026 = April 5 → Good Friday = April 3
        LoadProvider("DE").IsHoliday(new DateOnly(2026, 4, 3)).Should().BeTrue();
    }

    [Fact]
    public void DeHolidays_GermanUnityDay2026_NotShifted()
    {
        // Oct 3 2026 = Saturday — German fixed holidays have no observance shift
        LoadProvider("DE").GetHolidays(2026)
            .Should().Contain(h => h.Name.Contains("Einheit") && h.Date == new DateOnly(2026, 10, 3));
    }

    [Fact]
    public void DeHolidays_2026_ContainsNineHolidays()
        => LoadProvider("DE").GetHolidays(2026).Should().HaveCount(9);

    [Fact]
    public void DeBwHolidays_2026_ContainsTwelveHolidays()
        => LoadProvider("DE-BW").GetHolidays(2026).Should().HaveCount(12);

    // JP — Japan

    [Fact]
    public void JpHolidays_Region_IsJP()
        => LoadProvider("JP").Region.Should().Be("JP");

    [Fact]
    public void JpHolidays_NewYearsDay2026_IsHoliday()
        => LoadProvider("JP").IsHoliday(new DateOnly(2026, 1, 1)).Should().BeTrue();

    [Fact]
    public void JpHolidays_EmperorsBirthday_IsFeb23_Since2020()
        => LoadProvider("JP").IsHoliday(new DateOnly(2026, 2, 23)).Should().BeTrue();

    [Fact]
    public void JpHolidays_EmperorsBirthday_NotFeb23_Before2020()
    {
        // Feb 23 was not Emperor's Birthday before Reiwa era (since 2020)
        LoadProvider("JP").GetHolidays(2019)
            .Should().NotContain(h => h.Date == new DateOnly(2019, 2, 23) && h.Name.Contains("Emperor"));
    }

    [Fact]
    public void JpHolidays_SundaySubstitute_EmperorsBirthday2025()
    {
        // Feb 23 2025 is Sunday → Feb 24 (Monday) becomes substitute holiday
        LoadProvider("JP").IsHoliday(new DateOnly(2025, 2, 24)).Should().BeTrue();
    }

    [Fact]
    public void JpHolidays_2026_ContainsAtLeastSixteenHolidays()
        => LoadProvider("JP").GetHolidays(2026).Should().HaveCountGreaterThanOrEqualTo(16);

    [Fact]
    public void JpHolidays_AddHolidays_UnknownRegion_Throws()
    {
        var act = () => new ServiceCollection().AddHolidays("XX-INVALID");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*XX-INVALID*");
    }
}
