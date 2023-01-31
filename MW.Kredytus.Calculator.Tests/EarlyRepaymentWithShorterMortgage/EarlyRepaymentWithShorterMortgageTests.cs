﻿namespace MW.Kredytus.Calculator.Tests.EarlyRepaymentWithShorterMortgage;

public class EarlyRepaymentWithShorterMortgageTests
{
    [Test]
    public Task EarlyRepaymentAfterFirstInstallment()
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
        
        mortgage.MakeEarlyRepaymentAndShortenMortgage(20_000, firstInstallment);

        return Verify(mortgage);
    }
    
    [Test]
    public Task EarlyRepaymentAtHalfOfMortgage()
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
        var installmentWithEarlyRepayment = mortgage.Installments.Skip(mortgage.Installments.Count() / 2).First();

        mortgage.MakeEarlyRepaymentAndShortenMortgage(20_000, installmentWithEarlyRepayment);

        return Verify(mortgage);
    }
    
    [Test]
    public Task EarlyRepaymentEverySecondInstallment()
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

        for (var i = 0; i < mortgage.Installments.Count(); i+=2)
        {
            var installment = mortgage.Installments.ToArray()[i];
            mortgage.MakeEarlyRepaymentAndShortenMortgage(100, installment);
        }

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
        var totalInterestBeforeRepayment = mortgage.InterestSum;
        var firstInstallment = mortgage.Installments.First();
        
        mortgage.MakeEarlyRepaymentAndShortenMortgage(10_000, firstInstallment);
        
        mortgage.Installments.Should().HaveCount(326);
        mortgage.Installments.Should().AllSatisfy(installment =>
        {
            installment.TotalAmount.Should().BeApproximately(5525.61m, 5m);
        });
        mortgage.InterestSum.Should().BeApproximately(totalInterestBeforeRepayment - 116_764.26m, 1000m);
    }
}