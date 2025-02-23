using Newtonsoft.Json.Linq;

namespace StatementProcessor.Model;

public abstract class BankStatement
{
    protected abstract string DateField { get; }
    protected abstract string DescriptionField { get; }
    protected abstract string AmountField { get; }
    protected abstract IEnumerable<string> MemoFields { get; }
    
    protected abstract string? CardholderField { get; }
    protected abstract string SourceName { get; }

    public List<dynamic> RawTransactions { get; set; } = [];
    public abstract bool IsMatch(List<string> headers);
    
    public virtual void LoadTransactions(IEnumerable<dynamic> statementFile)
    {
        RawTransactions = statementFile.ToList();
    }

    public virtual List<Transaction> GetTransactions()
    {
        List<Transaction> transactions = new List<Transaction>();
        foreach (var rawTx in RawTransactions)
        {
            var txValues = rawTx as IDictionary<string, object>;

            var tx = new Transaction
            {
                TxDate = GetTransactionDate(txValues),
                Description = GetDescription(txValues),
                Amount = GetAmount(txValues),
                Memo = GetMemo(txValues),
                Source = SourceName,
                CardHolder = GetCardHolder(txValues)
            };
            tx.GenerateId(0);
            transactions.Add(tx);


            // Check if there are any duplicates and if so make them unique
            var duplicateGroups = transactions.GroupBy(t => t.UniqueId)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in duplicateGroups)
            {
                var counter = 1;
                foreach (var item in group)
                {
                    item.GenerateId(counter);
                    counter++;
                }
            }
        }
        return transactions;
    }

    private string? GetCardHolder(IDictionary<string, object> txValues)
    {
        return CardholderField is null ? string.Empty : GetStringValue(txValues, CardholderField);
    }

    protected virtual string? GetMemo(IDictionary<string, object> values)
    {
        if (!MemoFields.Any())
            return string.Empty;
        
        JObject memo = new JObject();
        foreach (var memoField in MemoFields)
        {
            memo.Add(memoField, JToken.FromObject(values[memoField]));
        }

        return memo.ToString().Replace("\t", "").Replace("\n", "");
    }

    protected virtual decimal GetAmount(IDictionary<string, object> values)
    {
        return Convert.ToDecimal(values["Amount"].ToString());
    }

    protected virtual string GetDescription(IDictionary<string, object> values)
    {
        return GetStringValue(values, DescriptionField);
    }

    protected virtual string GetStringValue(IDictionary<string, object> values, string fieldName)
    {
        return values[fieldName].ToString();
    }

    protected virtual DateTime GetTransactionDate(IDictionary<string, object> values)
    {
        return DateTime.Parse(values[DateField].ToString());
    }
}