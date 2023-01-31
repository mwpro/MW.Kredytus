namespace MW.Kredytus.Calculator;

public class Mortgage
{
    private readonly LinkedList<Installment> _installments = new LinkedList<Installment>();
    private readonly MortgageParams _mortgageParams;

    public IEnumerable<Installment> Installments => _installments.AsEnumerable();
    public decimal InterestSum => _installments.Sum(x => x.InterestRepayment);

    private Mortgage(MortgageParams mortgageParams)
    {
        _mortgageParams = mortgageParams;
    }

    public static Mortgage Create(MortgageParams mortgageParams)
    {
        var result = new Mortgage(mortgageParams);
        var date = GetNextInstallmentDate(mortgageParams.CalculationDate, mortgageParams.LastInstallmentDate);
        var installmentsCount = 0;
        while (date <= mortgageParams.LastInstallmentDate)
        {
            var installment = new Installment()
            {
                Date = date,
                InstallmentNumber = ++installmentsCount
            };
            result._installments.AddLast(installment);
            date = GetNextInstallmentDate(date, mortgageParams.LastInstallmentDate);
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
    
    public void MakeEarlyRepaymentAndLowerInstallments(decimal earlyRepaymentAmount, Installment firstInstallment)
    {
        var node = _installments.Find(firstInstallment);
        node.Value.SetEarlyRepayment(earlyRepaymentAmount);
        RecalculateInstallments(node.Next);
    }
    
    public void MakeEarlyRepaymentAndShortenMortgage(decimal earlyRepaymentAmount, Installment firstInstallment)
    {
        var node = _installments.Find(firstInstallment);
        node.Value.SetEarlyRepayment(earlyRepaymentAmount);
        node.Value.LowerNumberOfInstallments();
        RecalculateInstallments(node.Next);
    }
    
    private void RecalculateInstallments(LinkedListNode<Installment> installmentNode)
    {
        var currentInstallment = installmentNode;
        while (currentInstallment != null)
        {
            var installment = currentInstallment.Value;
            installment.Update(currentInstallment.Previous?.Value, _mortgageParams, _installments.Count);
            if (installment.RemainingAmount <= 0)
            {
                while (_installments.Last != currentInstallment)
                {
                    _installments.RemoveLast();
                }
            }
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

    public void IncreasedRepayment()
    {
        var currentInstallment = _installments.First;
        var firstInstallment = currentInstallment.Value.TotalAmount;
        while (currentInstallment != null)
        {
            var installment = currentInstallment.Value;
            installment.Update(currentInstallment.Previous?.Value, _mortgageParams, _installments.Count);
            if (installment.EarlyRepaymentAmount == 0)
            {
                installment.SetEarlyRepayment(firstInstallment - installment.TotalAmount);
            }
            if (installment.RemainingAmount <= 0)
            {
                while (_installments.Last != currentInstallment)
                {
                    _installments.RemoveLast();
                }
            }
            currentInstallment = currentInstallment.Next;
        }
    }
}