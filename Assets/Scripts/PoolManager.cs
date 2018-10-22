using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    static Dictionary<string, LinkedList<GameObject>> poolsDictionary;
    static Transform deactivatedObjectsContainer;

    public static void Initialization(Transform poolContainer)
    {
        deactivatedObjectsContainer = poolContainer;
        poolsDictionary = new Dictionary<string, LinkedList<GameObject>>();
    }

    public static GameObject GetObject(GameObject prefab)
    {
        GameObject result;

        if (!poolsDictionary.ContainsKey(prefab.tag))
        {
            poolsDictionary[prefab.tag] = new LinkedList<GameObject>();
        }

        if (poolsDictionary[prefab.tag].Count > 0)
        {
            result = poolsDictionary[prefab.tag].First.Value;
            poolsDictionary[prefab.tag].RemoveFirst();
            result.SetActive(true);
            return result;
        }

        result = GameObject.Instantiate(prefab);
        result.tag = prefab.tag;
        return result;
    }

    public static void PutGameObject(GameObject target)
    {
        poolsDictionary[target.tag].AddFirst(target);
        target.transform.parent = deactivatedObjectsContainer;
        target.SetActive(false);
    }

}
