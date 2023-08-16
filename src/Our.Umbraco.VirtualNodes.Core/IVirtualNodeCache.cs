namespace Our.Umbraco.VirtualNodes.Core;

public interface IVirtualNodeCache
{
    int GetRoute(string host, string path);
    void StoreRoute(string host, string path, int id);
    void ClearAll();
    void ClearHost(string host);
    void ClearRoute(string host, string path);
}