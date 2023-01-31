namespace MW.Kredytus.Calculator;

public class Installment
{
    public int InstallmentNumber { get; init; }
    public int NumberOfInstallmentsInTime { get; private set; }
    public DateOnly Date { get; init; }
    public decimal InitialAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public decimal BaseRate { get; private set; }
    public decimal BankMargin { get; private set; }
    public decimal EarlyRepaymentAmount { get; private set; }
    public decimal InterestRate => BaseRate + BankMargin;
    public decimal CapitalRepayment => TotalAmount - InterestRepayment;

    public decimal InterestRepayment { get; private set; }

    public decimal TotalAmount { get; private set; }

    public void Update(Installment? previousInstallment, MortgageParams mortgageParams, int numberOfInstallments)
    {
        InitialAmount = previousInstallment?.RemainingAmount ?? mortgageParams.RemainingAmount;
        NumberOfInstallmentsInTime = (previousInstallment?.NumberOfInstallmentsInTime - 1) ?? numberOfInstallments;
        BaseRate = previousInstallment?.BaseRate ?? mortgageParams.BaseRate;
        BankMargin = mortgageParams.BankMargin;
        if (InitialAmount / mortgageParams.CollateralValue > mortgageParams.LowLtvThreshold)
        {
            BankMargin += mortgageParams.LowLtvInterestIncrease;
        }
        CalculateInstallmentAmount();
    }

    private void CalculateInstallmentAmount()
    {
        var interestRate = InterestRate / 100m;
            
        InterestRepayment = CalculateInterestAmount();
        TotalAmount = CalculateInstallment();
        RemainingAmount = InitialAmount - CapitalRepayment - EarlyRepaymentAmount;

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

    public void ChangeBaseRate(decimal baseRate)
    {
        BaseRate = baseRate;
        CalculateInstallmentAmount();
    }
    
    public void SetEarlyRepayment(decimal earlyRepaymentAmount)
    {
        EarlyRepaymentAmount = earlyRepaymentAmount;
        CalculateInstallmentAmount();
    }

    public void LowerNumberOfInstallments()
    {
        var installment = this;
        var newNumberOfInstallmentsInTime = (int)Math.Round(Math.Log(
            (double)(1.0m - (installment.RemainingAmount * (installment.InterestRate / 100.0m)) /
                (installment.TotalAmount * 12.0m)), (double)(12.0m / (12.0m + installment.InterestRate / 100.0m))));
        Console.WriteLine($"{NumberOfInstallmentsInTime} -> {newNumberOfInstallmentsInTime}");
        NumberOfInstallmentsInTime = newNumberOfInstallmentsInTime;
    }
}