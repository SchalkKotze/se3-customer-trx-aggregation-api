public class CustomerTransactionFilter
{
    public string CustomerID { get; set; } = default!;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public String? SourceSystem { get; set; }
}
