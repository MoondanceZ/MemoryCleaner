namespace MemoryCleaner.Services;

public sealed record MemorySnapshot(ulong TotalBytes, ulong UsedBytes)
{
    public double UsedPercent => TotalBytes == 0
        ? 0
        : (double)UsedBytes / TotalBytes * 100;

    public ulong AvailableBytes => TotalBytes > UsedBytes
        ? TotalBytes - UsedBytes
        : 0;
}
