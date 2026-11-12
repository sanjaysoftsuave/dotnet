namespace EmployeeManagement.ConsoleApp;

// Value type: assigning or passing it copies the whole struct.
public readonly struct SalaryStats
{
    public SalaryStats(int count, decimal total, decimal min, decimal max)
    {
        Count = count;
        Total = total;
        Min = min;
        Max = max;
    }

    public int Count { get; }
    public decimal Total { get; }
    public decimal Min { get; }
    public decimal Max { get; }
    public decimal Average => Count == 0 ? 0 : Math.Round(Total / Count, 2);
}
