// Sprint 34: Testing - Unit tests for core domain logic

using EmployeeLifecyclePortal.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EmployeeLifecyclePortal.Tests.Domain;

public class EmployeeTests
{
    [Fact]
    public void CreateEmployee_WithValidData_ShouldSucceed()
    {
        // Arrange
        var employeeCode = "EMP001";
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var departmentId = Guid.NewGuid();

        // Act
        var employee = new Employee(employeeCode, firstName, lastName, email, null, departmentId);

        // Assert
        employee.EmployeeCode.Should().Be(employeeCode);
        employee.FirstName.Should().Be(firstName);
        employee.LastName.Should().Be(lastName);
        employee.Email.Should().Be(email);
        employee.FullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void AssignManager_WithValidManagerId_ShouldSucceed()
    {
        // Arrange
        var employee = new Employee("EMP001", "John", "Doe", "john@example.com", null, Guid.NewGuid());
        var managerId = Guid.NewGuid();

        // Act
        employee.AssignManager(managerId);

        // Assert
        employee.ManagerId.Should().Be(managerId);
    }

    [Fact]
    public void AssignManager_WithSameEmployeeId_ShouldThrow()
    {
        // Arrange
        var employee = new Employee("EMP001", "John", "Doe", "john@example.com", null, Guid.NewGuid());

        // Act & Assert
        var action = () => employee.AssignManager(employee.Id);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdatePhoneNumber_WithValidNumber_ShouldSucceed()
    {
        // Arrange
        var employee = new Employee("EMP001", "John", "Doe", "john@example.com", null, Guid.NewGuid());
        var phoneNumber = "+1-555-0100";

        // Act
        employee.UpdatePhoneNumber(phoneNumber);

        // Assert
        employee.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        // Arrange
        var employee = new Employee("EMP001", "John", "Doe", "john@example.com", null, Guid.NewGuid());

        // Act
        employee.Activate();

        // Assert
        employee.Status.ToString().Should().Be("Active");
    }

    [Fact]
    public void Terminate_ShouldSetStatusToTerminated()
    {
        // Arrange
        var employee = new Employee("EMP001", "John", "Doe", "john@example.com", null, Guid.NewGuid());

        // Act
        employee.Terminate();

        // Assert
        employee.Status.ToString().Should().Be("Terminated");
    }
}
