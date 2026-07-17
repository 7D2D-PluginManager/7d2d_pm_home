using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PluginManager.Api;

namespace HomePlugin;

public class JsonTeleportRepository : ITeleportRepository
{
    private const string Collection = "teleports";

    private readonly PluginStorage _storage;

    public JsonTeleportRepository(PluginStorage storage)
    {
        _storage = storage;
    }

    public void AddPoint(TeleportPoint point)
    {
        var points = GetPoints(point.UserId).ToList();
        points.RemoveAll(p => p.Name == point.Name);
        points.Add(point);
        Save(point.UserId, points);
    }

    public int RemovePoint(string userId, string name)
    {
        var points = GetPoints(userId).ToList();
        var removed = points.RemoveAll(p => p.Name == name);

        if (removed > 0)
            Save(userId, points);

        return removed;
    }

    public IEnumerable<TeleportPoint> GetPoints(string userId)
    {
        var json = _storage.Read(Collection, userId);

        if (string.IsNullOrEmpty(json))
            return new List<TeleportPoint>();

        return JsonConvert.DeserializeObject<List<TeleportPoint>>(json) ?? new List<TeleportPoint>();
    }

    public bool TryGetPoint(string userId, string name, out TeleportPoint point)
    {
        point = GetPoint(userId, name);
        return point != null;
    }

    public TeleportPoint GetPoint(string userId, string name)
    {
        return GetPoints(userId).FirstOrDefault(p => p.Name == name);
    }

    public int GetPointsCount(string userId)
    {
        return GetPoints(userId).Count();
    }

    private void Save(string userId, List<TeleportPoint> points)
    {
        if (points.Count == 0)
            _storage.Delete(Collection, userId);
        else
            _storage.Write(Collection, userId, JsonConvert.SerializeObject(points));
    }
}
