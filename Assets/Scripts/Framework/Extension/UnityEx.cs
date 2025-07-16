using UnityEngine.UI;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    /// <summary>
    /// ������Lua�ű����滻 onClick:AddListener Lua�����Dispose��ʱ����԰�ȫ�����callback
    /// </summary>
    public static void OnClickSet(this Button button, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // �Ƴ����м���(�رյ�ʱ�򲻻����٣�������������¼�)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener( () =>
        {
            func?.Call();
        });
    }

    /// <summary>
    /// ������Lua�ű����滻 onValueChanged.AddListener Lua�����Dispose��ʱ����԰�ȫ�����callback
    /// </summary>
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // �Ƴ����м���(�رյ�ʱ�򲻻����٣�������������¼�)
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((float value) =>
        {
            func?.Call(value);
        });
    }
}
