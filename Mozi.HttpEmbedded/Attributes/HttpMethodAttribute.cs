using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 禁止访问的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class ForbiddenMethodAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class HttpGetAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class HttpPostAttribute : Attribute
    {

    }
}
