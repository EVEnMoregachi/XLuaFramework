using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject
{
    // �������
    public Object Objuect;
    // ��������
    public string Name;
    // ���һ��ʹ��ʱ��
    public System.DateTime LastUseTime;

    public PoolObject(string name, Object obj)
    {
        Objuect = obj;
        Name = name;
        LastUseTime = System.DateTime.Now;
    }
}
