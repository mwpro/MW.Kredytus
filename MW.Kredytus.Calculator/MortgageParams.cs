namespace MW.Kredytus.Calculator;

public class MortgageParams
{
    public decimal RemainingAmount { get; set; }

    public DateOnly LastInstallmentDate { get; set; }

    public decimal BankMargin { get; set; }

    public decimal BaseRate { get; set; }

    public decimal EarlyRepaymentCommission { get; set; }

    public DateOnly EarlyRepaymentCommissionEndDate { get; set; }

    public decimal LowLtvThreshold { get; set; }
    public decimal LowLtvInterestIncrease { get; set; }
    public decimal CollateralValue { get; set; }
}