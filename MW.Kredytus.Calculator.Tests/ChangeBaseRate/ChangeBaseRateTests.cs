namespace MW.Kredytus.Calculator.Tests.ChangeBaseRate;

public class ChangeBaseRateTests
{
    [TestCase(0)]
    [TestCase(0.1)]
    [TestCase(1)]
    [TestCase(2.5)]
    [TestCase(-0.1)]
    [TestCase(-1)]
    [TestCase(-2.5)]
    [TestCase(-5)]
    public Task ChangeBaseRateOnFirstInstallment(decimal baseRateChange)
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
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.ChangeBaseRate(firstInstallment.BaseRate + baseRateChange, firstInstallment);

        return Verify(mortgage);
    }
        
    [TestCase(0)]
    [TestCase(0.1)]
    [TestCase(1)]
    [TestCase(2.5)]
    [TestCase(5)]
    [TestCase(-0.1)]
    [TestCase(-1)]
    [TestCase(-2.5)]
    [TestCase(-5)]
    public Task IncreaseBaseRateAtHalfOfMortgage(decimal baseRateChange)
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
        var firstMortgageWithChangedRate = mortgage.Installments.Skip(mortgage.Installments.Count()/2).First();
        
        mortgage.ChangeBaseRate(firstMortgageWithChangedRate.BaseRate + baseRateChange, firstMortgageWithChangedRate);

        return Verify(mortgage);
    }
        
    [TestCase(-2.5, 4355.03)]
    [TestCase(-2, 4580.37)]
    [TestCase(-1.5, 4810.36)]
    [TestCase(-1, 5044.73)]
    [TestCase(-0.5, 5283.23)]
    [TestCase(0.5, 5771.62)]
    [TestCase(1, 6021.02)]
    [TestCase(1.5, 6273.59)]
    [TestCase(2, 6529.08)]
    [TestCase(2.5, 6787.29)]
    public void Verify_against_uokik_calculator(decimal baseRateChange, decimal expectedInstallmentTotalAmount)
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
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.ChangeBaseRate(firstInstallment.BaseRate + baseRateChange, firstInstallment);
            
        mortgage.Installments.Should().HaveCount(349);
        mortgage.Installments.Should().AllSatisfy(installment =>
        {
            installment.TotalAmount.Should().BeApproximately(expectedInstallmentTotalAmount, 0.01m);
        });
    }
}