using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeObjectPool
{
    public Dictionary<int, GameObject> m_PoolCache;
    public ShapeObjectPool(List<GameObject> prefabList, Transform transform)
    {
        SpawnObject(prefabList, transform);
    }
    public void SpawnObject(List<GameObject> prefabList, Transform transform)
    {
        m_PoolCache ??= new Dictionary<int, GameObject>();

        foreach (var prefab in prefabList)
        {
            GameObject poolObject = Object.Instantiate(prefab);
            Shape shape = poolObject.GetComponent<Shape>();
            Outline outline = poolObject.GetComponent<Outline>();
            Object.Destroy(outline);

            if (shape != null)
            {
                m_PoolCache[shape.Id] = poolObject;
                poolObject.transform.SetParent(transform, false);
                poolObject.SetActive(false);
                poolObject.layer = 3;
            }
        }
    }
    public GameObject GetShapeObject(int id)
    {
        return m_PoolCache.TryGetValue(id, out GameObject existingPoolObject) ? existingPoolObject : null;
    }
}

