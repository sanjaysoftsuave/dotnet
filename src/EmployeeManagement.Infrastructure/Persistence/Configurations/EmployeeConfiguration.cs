using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

// Table-per-hierarchy: full-time employees and contractors share one Employees table,
// told apart by the EmploymentType discriminator column.
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.Ignore(e => e.EmploymentType); // computed from the CLR type
        builder.HasDiscriminator<string>("Kind")
            .HasValue<FullTimeEmployee>(nameof(EmploymentType.FullTime))
            .HasValue<Contractor>(nameof(EmploymentType.Contractor));
        builder.Property<string>("Kind").HasColumnName("EmploymentType").HasMaxLength(20);

        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(254);
        builder.Property(e => e.HireDate);

        // Relationships without navigation properties: the domain only holds ids.
        // Restrict: a department or manager cannot be deleted while rows still point at it.
        builder.HasOne<Department>().WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.ManagerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.LastName, e.FirstName });
        builder.HasIndex(e => e.HireDate);

        // The Address record is stored in the Employees table as Address_Street, Address_City, ...
        builder.OwnsOne(e => e.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(100);
            address.Property(a => a.City).HasMaxLength(60);
            address.Property(a => a.PostalCode).HasMaxLength(20);
            address.Property(a => a.Country).HasMaxLength(60);
        });
    }
}

internal sealed class FullTimeEmployeeConfiguration : IEntityTypeConfiguration<FullTimeEmployee>
{
    public void Configure(EntityTypeBuilder<FullTimeEmployee> builder)
    {
        builder.Property(e => e.AnnualSalary).HasPrecision(18, 2);
        builder.Property(e => e.BenefitsRate).HasPrecision(5, 4);
    }
}

internal sealed class ContractorConfiguration : IEntityTypeConfiguration<Contractor>
{
    public void Configure(EntityTypeBuilder<Contractor> builder)
    {
        builder.Property(c => c.HourlyRate).HasPrecision(18, 2);
        builder.Property(c => c.HoursPerYear);
        builder.Property(c => c.ContractEnd);
    }
}
