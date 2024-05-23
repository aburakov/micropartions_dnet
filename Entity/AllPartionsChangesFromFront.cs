namespace Micropartions.Entity;

public class MicropartionChangesFromFront
{
    public string MicropartionGuid { get; set; }
    public Box Box { get; set; }
}

public class Box
{
    public string BoxSerial { get; set; }
    public Skus[] Skus { get; set; }
}

public class Skus
{
    public string SkuSerial { get; set; }
    public PartionOperations PartionOperations { get; set; }
}

public class PartionOperations
{
    public string OperationGuid { get; set; }
    public int OperationNumber { get; set; }
}

