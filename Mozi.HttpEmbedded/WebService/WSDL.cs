using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Mozi.HttpEmbedded.WebService.Attributes;

namespace Mozi.HttpEmbedded.WebService
{
    /// <summary>
    /// WSDL描述文档
    /// </summary>
    internal class WSDL
    {

        public string NS_SOAP = "http://schemas.xmlsoap.org/wsdl/soap/";

        public string NS_BindingTransport = "http://schemas.xmlsoap.org/soap/http";

        public List<Namespace> Namespaces = new List<Namespace>()
        {
            new Namespace{ Prefix="soapenc", Uri="http://schemas.xmlsoap.org/soap/encoding/" },
            new Namespace{ Prefix="mime", Uri="http://schemas.xmlsoap.org/wsdl/mime/" },
            //new Namespace{ Prefix="soap", Uri="http://schemas.xmlsoap.org/wsdl/soap/" },
            new Namespace{ Prefix="soap12", Uri="http://schemas.xmlsoap.org/wsdl/soap12/" },
            new Namespace{ Prefix="http", Uri="http://schemas.xmlsoap.org/wsdl/http/" }
        };

        public string Prefix = "wsdl";
        public string Namespace = "http://schemas.xmlsoap.org/wsdl/";

        public string PrefixElement = "s";
        public string PrefixElementNamespace = "http://www.w3.org/2001/XMLSchema";

        public WebServiceAttribute ServiceAttribute;

        public string Description = "Mozi.WebService服务信息";
        public string ServiceName = "MoziWS";
        public string ServiceNamespace = "http://mozi.org";
        public string ServiceAddress = "http://127.0.0.1/";

        public bool AllowHttpAccess = true;

        public Types ApiTypes { get; set; }

        public WSDL()
        {
            ApiTypes = new Types();

            ServiceAttribute = new WebServiceAttribute()
            {

            };
        }
        /// <summary>
        /// 构建文档
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static string CreateDocument(WSDL desc)
        {
            XmlDocument doc = new XmlDocument();
            //declaration 
            var declare = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");

            doc.AppendChild(declare);

            //definitions
            var definitions = doc.CreateElement(desc.Prefix, "definitions", desc.Namespace);
            foreach (var ns in desc.Namespaces)
            {
                definitions.SetAttribute("xmlns:"+ns.Prefix, ns.Uri);
            }

            definitions.SetAttribute("xmlns:soap", desc.NS_SOAP);
            definitions.SetAttribute("xmlns:" + desc.PrefixElement, desc.PrefixElementNamespace);
            definitions.SetAttribute("targetNamespace", desc.ServiceNamespace);
            definitions.SetAttribute("xmlns:tns", desc.ServiceNamespace);

            doc.AppendChild(definitions);

            //documentation
            var documentation = doc.CreateElement(desc.Prefix, "documentation", desc.Namespace);
            documentation.AppendChild(doc.CreateTextNode(desc.Description));
            definitions.AppendChild(documentation);

            //types
            var types = doc.CreateElement(desc.Prefix, "types", desc.Namespace);
            definitions.AppendChild(types);

            var schema = doc.CreateElement(desc.PrefixElement, "schema", desc.PrefixElementNamespace);
            schema.SetAttribute("elementFormDefault", "qualified");
            schema.SetAttribute("targetNamespace", desc.ServiceNamespace);
            types.AppendChild(schema);

            //elements
            foreach (var s in desc.ApiTypes.Methods)
            {
                //params
                var elementIn = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                elementIn.SetAttribute("name", s.Name);
                var complexTypeIn = doc.CreateElement(desc.PrefixElement, "complexType", desc.PrefixElementNamespace);
                var sequenceIn = doc.CreateElement(desc.PrefixElement, "sequence", desc.PrefixElementNamespace);
                foreach (var r in s.GetParameters()) {
                    var elementParams = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                    elementParams.SetAttribute("minOccurs", "0");
                    elementParams.SetAttribute("maxOccurs", "1");
                    elementParams.SetAttribute("name", r.Name);
                    elementParams.SetAttribute("type", desc.PrefixElement + ":" + TypeConverter.FormDoNet(r.ParameterType));
                    sequenceIn.AppendChild(elementParams);
                }
                complexTypeIn.AppendChild(sequenceIn);
                elementIn.AppendChild(complexTypeIn);
                schema.AppendChild(elementIn);

                //return type
                var elementOut = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                elementOut.SetAttribute("name", s.Name + "Response");
                var returnValueType = s.ReturnType.UnderlyingSystemType;
                var isComplexType = false;
                var isEnumerable = false;
                var isMemberComplexType = false;
                var complexTypeOut = doc.CreateElement(desc.PrefixElement, "complexType", desc.PrefixElementNamespace);

                if (returnValueType != typeof(void))
                {
                    var sequenceOut = doc.CreateElement(desc.PrefixElement, "sequence", desc.PrefixElementNamespace);

                    var elementReturn = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                    elementReturn.SetAttribute("minOccurs", "0");
                    elementReturn.SetAttribute("maxOccurs", "1");
                    elementReturn.SetAttribute("name", returnValueType.Name + "Result");

                    if (IsPlatformDefinedType(returnValueType))
                    {
                        elementReturn.SetAttribute("type", desc.PrefixElement + ":" + TypeConverter.FormDoNet(returnValueType));
                    }
                    else
                    {
                        isComplexType = true;
                        isEnumerable = returnValueType.IsArray;
                        if (!isEnumerable) {
                            elementReturn.SetAttribute("type", "tns" + ":" + returnValueType.Name);
                        }
                        else
                        {
                            elementReturn.SetAttribute("type", "tns" + ":" + "ArrayOf" + returnValueType.GetElementType());
                        }

                    }
                    sequenceOut.AppendChild(elementReturn);
                    complexTypeOut.AppendChild(sequenceOut);
                }

                elementOut.AppendChild(complexTypeOut);

                schema.AppendChild(elementOut);

                //self define type
                if (isComplexType)
                {
                    XmlNamespaceManager xm = new XmlNamespaceManager(doc.NameTable);
                    xm.AddNamespace(desc.PrefixElement, desc.PrefixElementNamespace);
                    var exists = schema.SelectSingleNode("s:complexType[@name='" + returnValueType.Name + "']", xm);
                    //如果没有定义该类型
                    if (exists == null)
                    {
                        var complexTypeSelfDefine = doc.CreateElement(desc.PrefixElement, "complexType", desc.PrefixElementNamespace);
                        complexTypeSelfDefine.SetAttribute("name", isEnumerable ? ("ArrayOf" + returnValueType.Name) : returnValueType.Name);
                        var sequenceSelfDefine = doc.CreateElement(desc.PrefixElement, "sequence", desc.PrefixElementNamespace);
                        //非数组类型
                        if (!isEnumerable)
                        {
                            PropertyInfo[] props = returnValueType.GetProperties();
                            foreach (var r in props)
                            {
                                var elementReturnSelfDefine = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                                elementReturnSelfDefine.SetAttribute("minOccurs", "0");
                                elementReturnSelfDefine.SetAttribute("maxOccurs", "1");
                                elementReturnSelfDefine.SetAttribute("name", r.Name);
                                elementReturnSelfDefine.SetAttribute("type", desc.PrefixElement + ":" + TypeConverter.FormDoNet(r.GetType()));

                                sequenceSelfDefine.AppendChild(elementReturnSelfDefine);
                            }
                        }
                        //数组类型
                        else
                        {
                            var elementReturnSelfDefine = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                            elementReturnSelfDefine.SetAttribute("minOccurs", "0");
                            elementReturnSelfDefine.SetAttribute("maxOccurs", "unbounded");
                            elementReturnSelfDefine.SetAttribute("name", returnValueType.GetElementType().Name);
                            elementReturnSelfDefine.SetAttribute("nillable", "true");
                            elementReturnSelfDefine.SetAttribute("type", "tns:" + returnValueType.GetElementType().Name);
                            sequenceSelfDefine.AppendChild(elementReturnSelfDefine);
                        }
                        complexTypeSelfDefine.AppendChild(sequenceSelfDefine);

                        //数组类型包裹类型定义
                        if (isEnumerable)
                        {
                            var memberType = returnValueType.GetElementType();
                            var complexTypeMember = doc.CreateElement(desc.PrefixElement, "complexType", desc.PrefixElementNamespace);
                            complexTypeMember.SetAttribute("name", memberType.Name);
                            var sequenceMember = doc.CreateElement(desc.PrefixElement, "sequence", desc.PrefixElementNamespace);

                            PropertyInfo[] props = memberType.GetProperties();

                            if (!IsPlatformDefinedType(memberType))
                            {
                                foreach (var r in props)
                                {
                                    var elementReturnMember = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                                    elementReturnMember.SetAttribute("minOccurs", "0");
                                    elementReturnMember.SetAttribute("maxOccurs", "1");
                                    elementReturnMember.SetAttribute("name", r.Name);
                                    elementReturnMember.SetAttribute("type", desc.PrefixElement + ":" + TypeConverter.FormDoNet(r.GetType()));
                                    sequenceMember.AppendChild(elementReturnMember);
                                }
                            }
                            else
                            {
                                var elementReturnMember = doc.CreateElement(desc.PrefixElement, "element", desc.PrefixElementNamespace);
                                elementReturnMember.SetAttribute("minOccurs", "0");
                                elementReturnMember.SetAttribute("maxOccurs", "1");
                                elementReturnMember.SetAttribute("name", TypeConverter.FormDoNet(memberType));
                                elementReturnMember.SetAttribute("type", desc.PrefixElement + ":" + TypeConverter.FormDoNet(memberType.GetType()));
                                sequenceMember.AppendChild(elementReturnMember);
                            }

                            schema.AppendChild(complexTypeMember);
                        }

                        schema.AppendChild(complexTypeSelfDefine);
                    }
                }
            }

            //messages
            string[] soapActions = new string[] { "SoapIn", "SoapOut", "HttpGetIn", "HttpGetOut", "HttpPostIn", "HttpPostOut" };
            foreach (var s in desc.ApiTypes.Methods)
            {
                foreach (var act in soapActions)
                {
                    if (!desc.AllowHttpAccess && act.Contains("Http"))
                    {
                        continue;
                    }

                    var messageIn = doc.CreateElement(desc.Prefix, "message", desc.Namespace);
                    messageIn.SetAttribute("name", s.Name + act);

                    if (act == "SoapIn" || act == "SoapOut")
                    {
                        var partIn = doc.CreateElement(desc.Prefix, "part", desc.Namespace);
                        partIn.SetAttribute("name", "parameters");
                        if (act == "SoapIn")
                        {
                            partIn.SetAttribute("element", "tns:" + s.Name);
                        } else
                        {
                            partIn.SetAttribute("element", "tns:" + s.Name + "Response");
                        }
                        messageIn.AppendChild(partIn);
                    }
                    else if (act == "HttpGetIn" || act == "HttpPostIn")
                    {
                        foreach (var r in s.GetParameters())
                        {
                            var partParam = doc.CreateElement(desc.Prefix, "part", desc.Namespace);
                            partParam.SetAttribute("name", r.Name);
                            partParam.SetAttribute("type", desc.Prefix + ":" + TypeConverter.FormDoNet(r.ParameterType));
                            messageIn.AppendChild(partParam);
                        }
                    }
                    else
                    {
                        //TODO 未完成
                        var partIn = doc.CreateElement(desc.Prefix, "part", desc.Namespace);
                        partIn.SetAttribute("name", "parameters");
                        var returnType = s.ReturnType.UnderlyingSystemType;
                        if (IsPlatformDefinedType(returnType))
                        {
                            partIn.SetAttribute("element", "tns:" + s.Name);
                        }
                        else
                        {
                            partIn.SetAttribute("element", "tns:ArrayOf" + s.Name);
                        }
                        messageIn.AppendChild(partIn);
                    }
                    definitions.AppendChild(messageIn);

                }
            }

            //portTypes
            string[] portTypes = new string[] { "Soap", "HttpGet", "HttpPost" };
            foreach (var p in portTypes)
            {
                var portName = desc.ServiceName + p;
                var portType = doc.CreateElement(desc.Prefix, "portType", desc.Namespace);
                portType.SetAttribute("name", portName);
                if (!desc.AllowHttpAccess && p.Contains("Http"))
                {
                    continue;
                }
                foreach (var s in desc.ApiTypes.Methods)
                {
                    var operation = doc.CreateElement(desc.Prefix, "operation", desc.Namespace);
                    operation.SetAttribute("name", s.Name);
                    var documentationPorttType = doc.CreateElement(desc.Prefix, "documentation", desc.Namespace);
                    var input = doc.CreateElement(desc.Prefix, "input", desc.Namespace);
                    input.SetAttribute("message", "tns:" + s.Name + portName + "In");
                    var output = doc.CreateElement(desc.Prefix, "output", desc.Namespace);
                    output.SetAttribute("message", "tns" + s.Name + portName + "Out");
                    operation.AppendChild(documentationPorttType);
                    operation.AppendChild(input);
                    operation.AppendChild(output);
                    portType.AppendChild(operation);
                    definitions.AppendChild(portType);

                }
            }
            //binding
            string[] bindingTypes = new string[] { "Soap", "Soap12", "HttpGet", "HttpPost" };
            foreach (var p in bindingTypes)
            {
                var portName = desc.ServiceName + p;
                if (!desc.AllowHttpAccess && p.Contains("Http"))
                {
                    continue;
                }

                var binding = doc.CreateElement(desc.Prefix, "binding", desc.Namespace);
                binding.SetAttribute("name", portName);
                if (p.Contains("Soap"))
                {
                    binding.SetAttribute("type", "tns:" + desc.ServiceName + "Soap");
                }
                else
                {
                    binding.SetAttribute("type", "tns:" + portName);
                }
                var soapBinding = doc.CreateElement("soap", "binding", desc.NS_SOAP);
                soapBinding.SetAttribute("transport", desc.NS_SOAP);
                binding.AppendChild(soapBinding);

                foreach (var r in desc.ApiTypes.Methods)
                {

                    var operation = doc.CreateElement(desc.Prefix, "operation", desc.Namespace);
                    operation.SetAttribute("name", r.Name);
                    var soapOperation = doc.CreateElement("soap", "operation", desc.NS_SOAP);
                    soapOperation.SetAttribute("soapAction", desc.ServiceNamespace + "/" + r.Name);
                    soapOperation.SetAttribute("style", "document");

                    //input 
                    var input = doc.CreateElement(desc.Prefix, "input", desc.Namespace);
                    var bodyInput = doc.CreateElement("soap", "body", desc.NS_SOAP);
                    bodyInput.SetAttribute("use", "literal");
                    input.AppendChild(bodyInput);

                    //output
                    var output = doc.CreateElement(desc.Prefix, "output", desc.Namespace);
                    var bodyOutput = doc.CreateElement("soap", "body", desc.NS_SOAP);
                    bodyOutput.SetAttribute("use", "literal");
                    output.AppendChild(bodyOutput);

                    operation.AppendChild(input);
                    operation.AppendChild(output);
                    operation.AppendChild(soapOperation);
                    binding.AppendChild(operation);
                }

                definitions.AppendChild(binding);
            }

            //service
            var service = doc.CreateElement(desc.Prefix, "service", desc.Namespace);
            service.SetAttribute("name", desc.ServiceName);
            var serviceDocumentation = doc.CreateElement(desc.Prefix, "documentation", desc.Namespace);
            serviceDocumentation.AppendChild(doc.CreateTextNode(desc.Description));
            service.AppendChild(serviceDocumentation);
            foreach (var p in bindingTypes)
            {
                var port = doc.CreateElement(desc.Prefix, "port", desc.Namespace);
                port.SetAttribute("name", desc.ServiceName + p);
                port.SetAttribute("binding", "tns:" + desc.ServiceName+p);
                var address = doc.CreateElement("soap", "address", desc.NS_SOAP);
                address.SetAttribute("location", desc.ServiceAddress+desc.ServiceName);
                port.AppendChild(address);
                service.AppendChild(port);
            }
            definitions.AppendChild(service);
            return doc.OuterXml;

        }
        /// <summary>
        /// 判断是否平台预先定义类型
        /// 基本类型+String
        /// </summary>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public static bool IsPlatformDefinedType(Type memberType)
        {
            return memberType.IsPrimitive || memberType == Type.GetType("System.String");
        }
        public class Types
        {
            public List<MethodInfo> Methods = new List<MethodInfo>();

            private class Element
            {
                public string Name = "";
            }

            public class ElementSequence
            {
                public string minOccurs = "";
                public string maxOccurs = "";
                public string name = "";
                public string type = "";
            }
        }

        private class Message
        {
            public class Part
            {
                public string Name { get; set; }
                public string Element { get; set; }
            }
        }

        private class PortType
        {

        }

        private class ServiceType
        {

        }

        private class Binding
        {

        }

        private class Service
        {

        }
    }

    public class TypeConverter
    {
        public static string FormDoNet(Type valueType)
        {
            string result = "string";
            string type = valueType.ToString();
            switch (type)
            {
                case "System.String":
                    result = "string";
                    break;
                case "System.UInt16":
                case "System.Int32":
                    result = "int";
                    break;
                case "System.Int16":
                    result = "short";
                    break;
                case "System.UInt32":
                case "System.Int64":
                    result = "long";
                    break;
                case "System.Boolean":
                    result = "boolean";
                    break;
                case "System.Float":
                    result = "float";
                    break;
                case "System.Double":
                    result = "double";
                    break;
                case "System.Decimal":
                    result = "decimal";
                    break;
                case "System.Byte":
                    result = "byte";
                    break;
                case "System.DateTime":
                    result = "datetime";
                    break;
                default:
                    result = "string";
                    break;
            }
            return result;
        }
    }

    public static class XmlDocumentExtension
    {
        //public static XmlNode Append(this XmlNode node,XmlNode child)
        //{
        //    var node2 = node.AppendChild(child);            
        //    return node2;
        //}

        //public static XmlNode Put(this XmlDocument doc,XmlNode child,XmlNode parent)
        //{
        //    XmlNode node2 = null;
        //    if (parent != null)
        //    {
        //        node2 = parent.AppendChild(child);
        //        parent.AppendChild(doc.CreateSignificantWhitespace("\r\n"));
        //    }
        //    else
        //    {
        //        node2 = doc.AppendChild(child);
        //        doc.AppendChild(doc.CreateSignificantWhitespace("\r\n"));
        //    }
        //    return node2;
        //}
    }
}
