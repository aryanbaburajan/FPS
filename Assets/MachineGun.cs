using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Item
{
    public void Drop(Transform transform)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, Mathf.Infinity))
        {
            transform.localEulerAngles = new Vector3(0f, Random.Range(-180f, 180f), 90f);
            transform.position = hitInfo.point;
        }
    }
}

public class MachineGun : MonoBehaviour, Item
{
    void Start()
    {
        (this as Item).Drop(transform);
    }
}
