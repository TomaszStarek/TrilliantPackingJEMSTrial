using System.Collections.Generic;
using System;

public class InQueueRouteStep
{
    public int InQueueRouteStepId { get; set; }
    public string InQueueRouteStepName { get; set; }
    public string InQueueRouteStepRouteName { get; set; }
}

public class AssembledItem
{
    public object AssembleType { get; set; }
    public int MaterialId { get; set; }
    public string Material { get; set; }
    public string MaterialType { get; set; }
    public int EntityId { get; set; }
    public string Identifier { get; set; }
    public string EventType { get; set; }
    public int Quantity { get; set; }
    public string Operator { get; set; }
    public DateTime DateTime { get; set; }
}

public class OperationHistory
{
    public int WipProcessStepHistoryId { get; set; }
    public int WipId { get; set; }
    public string Factory { get; set; }
    public string ManufacturingArea { get; set; }
    public int RouteId { get; set; }
    public string RouteName { get; set; }
    public int RouteStepTypeId { get; set; }
    public string RouteStepTypeName { get; set; }
    public string StationType { get; set; }
    public int RouteStepId { get; set; }
    public string RouteStepName { get; set; }
    public DateTime InQueueDateTime { get; set; }
    public DateTime ArrivalDateTime { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Resource { get; set; }
    public string ResourceType { get; set; }
    public string ResourceFunction { get; set; }
    public string Operator { get; set; }
    public string OperationStatus { get; set; }
    public int WipReturnCount { get; set; }
    public int FixtureId { get; set; }
    public object FixtureName { get; set; }
    public int CellNumber { get; set; }
    public string ProcessLoopCount { get; set; }
    public string LoopCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public IList<object> Failures { get; set; }
    public IList<object> Defects { get; set; }
    public IList<object> Reworks { get; set; }
    public IList<AssembledItem> AssembledItems { get; set; }
}

public class Wip
{
    public int WipId { get; set; }
    public string SerialNumber { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int DivisionId { get; set; }
    public string DivisionName { get; set; }
    public int ProductFamilyId { get; set; }
    public string ProductFamilyName { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; }
    public int AssemblyId { get; set; }
    public string AssemblyNumber { get; set; }
    public string AssemblyDescription { get; set; }
    public string AssemblyRevision { get; set; }
    public string AssemblyVersion { get; set; }
    public int PlannedOrderId { get; set; }
    public string PlannedOrderNumber { get; set; }
    public bool IsOnHold { get; set; }
    public bool IsScrapped { get; set; }
    public bool IsPacked { get; set; }
    public bool IsReferenceUnit { get; set; }
    public bool IsAssembled { get; set; }
    public string WipStatus { get; set; }
    public DateTime WipCreationDate { get; set; }
    public object ParentWip { get; set; }
    public IList<InQueueRouteStep> InQueueRouteSteps { get; set; }
    public object Panel { get; set; }
    public IList<OperationHistory> OperationHistories { get; set; }
}

public class Example
{
    public IList<Wip> Wips { get; set; }
}
