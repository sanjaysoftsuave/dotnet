namespace EmployeeManagement.Domain.Services;

public sealed class EmployeeTransferredEventArgs(int employeeId, int fromDepartmentId, int toDepartmentId) : EventArgs
{
    public int EmployeeId { get; } = employeeId;
    public int FromDepartmentId { get; } = fromDepartmentId;
    public int ToDepartmentId { get; } = toDepartmentId;
}
