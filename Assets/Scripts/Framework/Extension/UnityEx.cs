using UnityEngine.UI;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    /// <summary>
    /// 用于在Lua脚本中替换 onClick:AddListener Lua虚拟机Dispose的时候可以安全的清除callback
    /// </summary>
    public static void OnClickSet(this Button button, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // 移除所有监听(关闭的时候不会销毁，否则会监听多个事件)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener( () =>
        {
            func?.Call();
        });
    }

    /// <summary>
    /// 用于在Lua脚本中替换 onValueChanged.AddListener Lua虚拟机Dispose的时候可以安全的清除callback
    /// </summary>
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // 移除所有监听(关闭的时候不会销毁，否则会监听多个事件)
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((float value) =>
        {
            func?.Call(value);
        });
    }
}
