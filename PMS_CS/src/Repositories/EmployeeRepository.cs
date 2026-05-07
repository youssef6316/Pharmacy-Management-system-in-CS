using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class EmployeeRepository
{
    public bool AddEmployee(Employee employee)
    {
        const string query = @"
            INSERT INTO EMPLOYEE (UserID, Salary, JobType)
            VALUES (@UserId, @Salary, @JobType)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId",  employee.UserId);
        cmd.Parameters.AddWithValue("@Salary",  employee.Salary);
        cmd.Parameters.AddWithValue("@JobType", employee.JobType);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    public Employee? GetEmployeeById(int userId)
    {
        const string query = @"
            SELECT e.UserID, e.Salary, e.JobType,
                   u.Username, u.Email, u.Phone, u.IsActive
            FROM EMPLOYEE e
            INNER JOIN [USER] u ON e.UserID = u.UserID
            WHERE e.UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read()) return MapEmployee(reader);
        return null;
    }

    public List<Employee> GetByJobType(string jobType)
    {
        const string query = @"
            SELECT e.UserID, e.Salary, e.JobType,
                   u.Username, u.Email, u.Phone, u.IsActive
            FROM EMPLOYEE e
            INNER JOIN [USER] u ON e.UserID = u.UserID
            WHERE e.JobType = @JobType
            ORDER BY u.Username";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@JobType", jobType);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var employees = new List<Employee>();
        while (reader.Read())
            employees.Add(MapEmployee(reader));

        return employees;
    }

    public bool UpdateSalary(int userId, double newSalary)
    {
        const string query = @"
            UPDATE EMPLOYEE SET Salary = @Salary
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Salary", newSalary);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    private static Employee MapEmployee(SqlDataReader reader)
    {
        var emp = new Employee
        {
            UserId  = Convert.ToInt32(reader["UserID"]),
            Salary  = Convert.ToDouble(reader["Salary"]),
            JobType = reader["JobType"].ToString()!
        };

        emp.UserInfo = new User
        {
            UserId   = emp.UserId,
            Username = reader["Username"].ToString()!,
            Email    = reader["Email"].ToString()!,
            Phone    = reader["Phone"].ToString()!,
            IsActive = Convert.ToBoolean(reader["IsActive"])
        };

        return emp;
    }
}