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
        
        result.RecalculateInstallments(result._installments.First);

        return result;
    }

    public void ChangeBaseRate(decimal baseRate, Installment firstInstallment)
    {
        var node = _installments.Find(firstInstallment);
        node.Value.ChangeBaseRate(baseRate);
        RecalculateInstallments(node.Next);
    }
    
    private void RecalculateInstallments(LinkedListNode<Installment> installmentNode)
    {
        var currentInstallment = installmentNode;
        while (currentInstallment != null)
        {
            var installment = currentInstallment.Value;
            installment.Update(currentInstallment.Previous?.Value, _mortgageParams, _installments.Count);
            
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