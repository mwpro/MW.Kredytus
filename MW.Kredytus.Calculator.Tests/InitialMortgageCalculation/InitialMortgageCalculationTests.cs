namespace MW.Kredytus.Calculator.Tests.InitialMortgageCalculation;

public class InitialMortgageCalculationTests
{
    [Test]
    public Task MortgageWithoutLowLtvInterestIncrease()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            CalculationDate = new DateOnly(2023, 01, 31),
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);

        return Verify(mortgage);
    }
    
    [Test]
    public Task MortgageWithLowLtvInterestIncrease()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            CalculationDate = new DateOnly(2023, 01, 31),
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            LowLtvThreshold = 0.8m,
            LowLtvInterestIncrease = 0.2m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);

        return Verify(mortgage);
    }
    
    [Test]
    public void Verify_against_uokik_calculator()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            CalculationDate = new DateOnly(2023, 01, 31),
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);

        mortgage.Installments.Should().HaveCount(349);
        mortgage.Installments.Should().AllSatisfy(installment =>
        {
            installment.TotalAmount.Should().BeApproximately(5525.61m, 0.01m);
        });
        var firstInstallment = mortgage.Installments.First();
        firstInstallment.InterestRepayment.Should().BeApproximately(5112.44m, 0.01m);
        firstInstallment.CapitalRepayment.Should().BeApproximately(413.17m, 0.01m);
    }
}