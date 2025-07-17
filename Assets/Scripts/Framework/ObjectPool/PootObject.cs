using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject
{
    // �������
    public Object Object;
    // ��������
    public string Name;
    // ���һ��ʹ��ʱ��
    public System.DateTime LastUseTime;

    public PoolObject(string name, Object obj)
    {
        Object = obj;
        Name = name;
        LastUseTime = System.DateTime.Now;
    }
}
