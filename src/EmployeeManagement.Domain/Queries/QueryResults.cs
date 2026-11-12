namespace EmployeeManagement.Domain.Queries;

public sealed record DepartmentCostSummary(string Department, int HeadCount, decimal TotalCost, decimal Budget)
{
    public bool IsOverBudget => TotalCost > Budget;
}

public sealed record ManagerSummary(int ManagerId, string ManagerName, int DirectReports);
