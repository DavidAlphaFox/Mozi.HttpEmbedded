using System;

namespace Mozi.HttpEmbedded.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class WebServiceAttribute:Attribute
    {
        public string Namespace = "";
        public string DocumentName = "";
    }
}
