namespace Finanzas.Models;

public sealed class FinanceData
{
    public decimal MonthlyBudget { get; set; } = 0;
    public List<FinanceMovement> Movements { get; set; } = new();
    public List<FinanceGoal> Goals { get; set; } = new();
    public Dictionary<string, decimal> CategoryBudgets { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
public sealed class FinanceMovement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Kind { get; set; } = "Gasto";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string Category { get; set; } = "Gastos personales";
    public string Note { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
public sealed class FinanceGoal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal Saved { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
