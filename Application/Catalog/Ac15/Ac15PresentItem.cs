namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record Ac15PresentItem(
    uint Index,
    uint ItemType,
    uint ItemNumber,
    uint DonPoint);
