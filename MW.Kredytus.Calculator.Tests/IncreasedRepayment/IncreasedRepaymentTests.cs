namespace MW.Kredytus.Calculator.Tests.IncreasedRepayment;

public class IncreasedRepaymentTests
{
    [Test]
    public Task VerifySingleInitialRepayment()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            LowLtvThreshold = 0.8m,
            LowLtvInterestIncrease = 0.2m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.MakeEarlyRepaymentAndLowerInstallments(20_000, firstInstallment);
        mortgage.IncreasedRepayment();

        return Verify(mortgage);
    }
    
    [Test]
    public Task VerifyMultipleInitialRepayment()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            LowLtvThreshold = 0.8m,
            LowLtvInterestIncrease = 0.2m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.MakeEarlyRepaymentAndLowerInstallments(20_000, firstInstallment);
        mortgage.MakeEarlyRepaymentAndLowerInstallments(20_000, mortgage.Installments.Skip(12).First());
        mortgage.IncreasedRepayment();

        return Verify(mortgage);
    }
    
    [Test]
    public void TestMultipleInitialRepayment()
    {
        var mortgageParams = new MortgageParams()
        {
            RemainingAmount = 685_467,
            LastInstallmentDate = new DateOnly(2052, 02, 05),
            BankMargin = 1.65m,
            BaseRate = 7.30m,
            LowLtvThreshold = 0.8m,
            LowLtvInterestIncrease = 0.2m,
            CollateralValue = 760_000m
        };
        var mortgage = Mortgage.Create(mortgageParams);
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.MakeEarlyRepaymentAndLowerInstallments(20_000, firstInstallment);
        mortgage.MakeEarlyRepaymentAndLowerInstallments(20_000, mortgage.Installments.Skip(12).First());
        mortgage.IncreasedRepayment();

        mortgage.Installments.Should().HaveCountLessThan(349);
        mortgage.Installments
            .Where(x => x.InstallmentNumber != 1 && x.InstallmentNumber != 13)
            .Should().AllSatisfy(installment =>
        {
            (installment.TotalAmount + installment.EarlyRepaymentAmount).Should().BeApproximately(5623.59m, 0.01m);;
        });
    }
}