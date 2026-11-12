using EmployeeManagement.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(60).IsRequired();
        builder.HasIndex(d => d.Name).IsUnique();

        // Code has no setter, so it is mapped explicitly; EF writes its backing field.
        builder.Property(d => d.Code).HasMaxLength(10).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();

        builder.Property(d => d.AnnualBudget).HasPrecision(18, 2);
    }
}
