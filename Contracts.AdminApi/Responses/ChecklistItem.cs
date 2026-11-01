namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public enum ChecklistLevel { Problem = 0, Warning = 1, Ok = 2 }

/// <summary>One line of Access › Overview's checklist: what's wrong (or fine), what it means, how to fix it.</summary>
public record ChecklistItem(ChecklistLevel Level, string Title, string Detail);
