using Index = MW.Kredytus.Pages.Index;

namespace MW.Kredytus.Calculator;

public class Installment
{
    public int InstallmentNumber { get; init; }
    public int NumberOfInstallmentsInTime { get; set; }
    public DateOnly Date { get; init; }
    public decimal InitialAmount { get; set; }
    public decimal RemainingAmount { get; private set; }
    public decimal BaseRate { get; set; }
    public decimal BankMargin { get; private set; }
    public decimal InterestRate => BaseRate + BankMargin;
    public decimal CapitalRepayment => TotalAmount - InterestRepayment;

    public decimal InterestRepayment { get; private set; }

    public decimal TotalAmount { get; private set; }

    public void Update()
    {
        var interestRate = InterestRate / 100m;
            
        InterestRepayment = CalculateInterestAmount();
        TotalAmount = CalculateInstallment();
        RemainingAmount = InitialAmount - CapitalRepayment;

        decimal CalculateInterestAmount()
        {
            return (interestRate) / 12.0m * InitialAmount;
        }
        decimal CalculateInstallment()
        {
            return (InitialAmount * interestRate)
                   /
                   (12.0m * (1.0m - (decimal)Math.Pow((double)(12.0m / (12.0m + interestRate)), NumberOfInstallmentsInTime)));
        }
    }
    
    public void Update2(Installment? previousInstallment, Index.MortgageParams mortgageParams, int numberOfInstallments)
    {
        InitialAmount = previousInstallment?.RemainingAmount ?? mortgageParams.RemainingAmount;
        NumberOfInstallmentsInTime = numberOfInstallments - (this.InstallmentNumber - 1);
        BaseRate = previousInstallment?.BaseRate ?? mortgageParams.BaseRate;
        BankMargin = mortgageParams.BankMargin;
        if (InitialAmount / mortgageParams.CollateralValue > mortgageParams.LowLtvThreshold)
        {
            BankMargin += mortgageParams.LowLtvInterestIncrease;
        }
        Update();
    }
}