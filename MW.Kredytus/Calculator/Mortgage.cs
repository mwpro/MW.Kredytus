using Index = MW.Kredytus.Pages.Index;

namespace MW.Kredytus.Calculator;

public class Mortgage
{
    private readonly LinkedList<Installment> _installments = new LinkedList<Installment>();
    private readonly Index.MortgageParams _mortgageParams;

    public IEnumerable<Installment> Installments => _installments.AsEnumerable();
    public decimal InterestSum => _installments.Sum(x => x.InterestRepayment);

    private Mortgage(Index.MortgageParams mortgageParams)
    {
        _mortgageParams = mortgageParams;
    }

    public static Mortgage Create(Index.MortgageParams mortgageParams)
    {
        var result = new Mortgage(mortgageParams);
        var date = GetNextInstallmentDate(DateOnly.FromDateTime(DateTime.Now.Date), mortgageParams.LastInstallmentDate);
        var installmentsCount = 0;
        while (date <= mortgageParams.LastInstallmentDate)
        {
            date = GetNextInstallmentDate(date, mortgageParams.LastInstallmentDate);
            var installment = new Installment()
            {
                Date = date,
                InstallmentNumber = ++installmentsCount
            };
            result._installments.AddLast(installment);
        }
        
        result.Update(result._installments.First);

        return result;
    }
    
    public void UpdateNextInstallmentsBaseRate(Installment firstInstallment)
    {
        var node = _installments.Find(firstInstallment);
        firstInstallment.Update();
        Update(node.Next);
    }
    
    private void Update(LinkedListNode<Installment> installmentNode)
    {
        var currentInstallment = installmentNode;
        while (currentInstallment != null)
        {
            var installment = currentInstallment.Value;
            installment.InitialAmount = currentInstallment.Previous?.Value.RemainingAmount ?? _mortgageParams.RemainingAmount;
            installment.NumberOfInstallmentsInTime = _installments.Count - (currentInstallment.Value.InstallmentNumber - 1);
            installment.BaseRate = currentInstallment.Previous?.Value.BaseRate ?? _mortgageParams.BaseRate;
            installment.BankMargin = _mortgageParams.BankMargin;
            if (installment.InitialAmount / _mortgageParams.CollateralValue > _mortgageParams.LowLtvThreshold)
            {
                installment.BankMargin += _mortgageParams.LowLtvInterestIncrease;
            }
            installment.Update();
            
            currentInstallment = currentInstallment.Next;
        }
    }

    private static DateOnly GetNextInstallmentDate(DateOnly onOrAfterDate, DateOnly lastInstallmentDate)
    {
        if (onOrAfterDate.Day >= lastInstallmentDate.Day)
        {
            var nextMonth = onOrAfterDate.AddMonths(1);
            return new DateOnly(nextMonth.Year, nextMonth.Month, lastInstallmentDate.Day);
        }
        return new DateOnly(onOrAfterDate.Year, onOrAfterDate.Month, lastInstallmentDate.Day);
    }
}