using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace StatementProcessor.Model;

public class StartingStatement(ILogger<StartingStatement> logger) : BankStatement(logger)
{
    protected override string DateField { get; } = "Date";
    protected override string DescriptionField { get; } = "Counter Party";
    protected override string AmountField { get; } = "Amount (GBP)";
    protected override IEnumerable<string> MemoFields { get; } = ["Reference", "Type", "Balance (GBP)" ];
    protected override string? CardholderField { get; }
    protected override string SourceName { get; } = "Starling Joint";

    protected override decimal GetAmount(IDictionary<string, object> values)
    {
        // Invert the values so expenditure is positive
        return -base.GetAmount(values);
    }

    public override bool IsMatch(List<string> headers)
    {
        return headers.SequenceEqual(new List<string> { "Date", "Counter Party", "Reference", "Type", "Amount (GBP)", "Balance (GBP)",
            "Spending Category", "Notes" });
    }
}