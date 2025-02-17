namespace StatementProcessor.Model;

public class TescoMastercard : BankStatement
{
    protected override string DateField { get; } = "Transaction Date";
    protected override string DescriptionField { get; } = "Merchant";
    protected override string AmountField { get; } = "Billing Amount";
    protected override IEnumerable<string> MemoFields { get; } = new List<string> { "Posting Date","Merchant City","Merchant County","Merchant Postal Code","Reference Number","Debit/Credit Flag","SICMCC Code" };
    
    protected override string CardholderField { get; }
    protected override string SourceName { get; } = "TescoMastercard";

    public override bool IsMatch(List<string> headers)
    {
        return headers.SequenceEqual(new List<string> { "Transaction Date","Posting Date","Billing Amount","Merchant","Merchant City","Merchant County","Merchant Postal Code","Reference Number","Debit/Credit Flag","SICMCC Code" });
    }

    protected override decimal GetAmount(IDictionary<string, object> values)
    {
        return Convert.ToDecimal(GetStringValue(values,AmountField).Replace("\ufffd", "").Trim());
    }
}