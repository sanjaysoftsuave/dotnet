namespace EmployeeManagement.Domain.Queries;

// Value type: small, immutable and copied by value. default(CostStatistics) means "no employees".
public readonly record struct CostStatistics(int Count, decimal Total, decimal Min, decimal Max)
{
    public decimal Average => Count == 0 ? 0 : Math.Round(Total / Count, 2);

    public CostStatistics Add(decimal cost) => Count == 0
        ? new CostStatistics(1, cost, cost, cost)
        : new CostStatistics(Count + 1, Total + cost, Math.Min(Min, cost), Math.Max(Max, cost));
}
