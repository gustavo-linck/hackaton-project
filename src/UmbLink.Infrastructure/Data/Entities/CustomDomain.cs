namespace UmbLink.Infrastructure.Data.Entities;

public class CustomDomain
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string CnameTarget { get; set; } = "cname.umblink.com";
    public DomainStatus Status { get; set; } = DomainStatus.Pending;
    public Page Page { get; set; } = null!;
}

public enum DomainStatus { Pending, Active, Failed }
