using System;

/// <summary>
/// 属性标记还需要统筹设计
/// </summary>
namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// API属性标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BasicApiAttribute : Attribute
    {

    }
}
