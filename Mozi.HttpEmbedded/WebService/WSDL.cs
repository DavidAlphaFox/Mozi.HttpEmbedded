using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Mozi.HttpEmbedded.WebService
{
    /// <summary>
    /// WSDL描述文档
    /// </summary>
    public class WSDL
    {

        public string NS_soapenc = "http://schemas.xmlsoap.org/soap/encoding/";
        public string NS_mime = "http://schemas.xmlsoap.org/wsdl/mime/";
        public string NS_soap = "http://schemas.xmlsoap.org/wsdl/soap/";
        public string NS_soap12 = "http://schemas.xmlsoap.org/wsdl/soap12/";
        public string NS_http = "http://schemas.xmlsoap.org/wsdl/http/";

        public string BindingTransport = "http://schemas.xmlsoap.org/soap/http";

        public string Prefix = "wsdl";

        public string WSDLNamespace = "http://schemas.xmlsoap.org/wsdl/";

        public string Namespace = "http://mozi.org";

        public string PrefixElement = "s";
        public string PrefixElementNamespace = "http://www.w3.org/2001/XMLSchema";

        public string Description = "Mozi.WebService服务信息";
        public string Name = "Mozi.WebService";
        public string ServiceAddress = "http://127.0.0.1/runtime/soap?action=wsdl";

        public bool AllowHttpAccess = true;

        public Types ApiTypes { get; set; }

        public WSDL()
        {
            ApiTypes = new Types();
        }
        /// <summary>
        /// 构建文档
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static string CreateDocument(WSDL document)
        {
            XmlDocument doc = new XmlDocument();

            //declaration 
            var declare=doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            doc.AppendChild(declare);
            //definitions
            var definitions = doc.CreateElement(document.Prefix, "definitions", document.WSDLNamespace);
            definitions.SetAttribute("xmlns:soapenc", document.NS_soapenc);
            definitions.SetAttribute("xmlns:mime",  document.NS_mime);
            definitions.SetAttribute("xmlns:soap", document.NS_soap);
            definitions.SetAttribute("xmlns:"+document.PrefixElement, document.PrefixElementNamespace);
            definitions.SetAttribute("xmlns:soap12", document.NS_soap12);
            definitions.SetAttribute("xmlns:http", document.NS_http);
            definitions.SetAttribute("targetNamespace", document.Namespace);
            definitions.SetAttribute("xmlns:tns", document.Namespace);
            doc.AppendChild(definitions);

            //documentation
            var documentation = doc.CreateElement(document.Prefix, "documentation", document.WSDLNamespace);
            documentation.InnerText=document.Description;
            definitions.AppendChild(documentation);

            //types
            var types= doc.CreateElement(document.Prefix, "types", document.WSDLNamespace);
            definitions.AppendChild(types);
            var schema=doc.CreateElement(document.PrefixElement, "schema",document.PrefixElementNamespace);
            schema.SetAttribute("elementFormDefault", "qualified");
            schema.SetAttribute("targetNamespace", document.Namespace);
            types.AppendChild(schema);

            //elements
            foreach(var s in document.ApiTypes.Methods)
            {
                //params
                var elementIn = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                elementIn.SetAttribute("name", s.Name);
                var complexTypeIn=doc.CreateElement(document.PrefixElement, "complexType", document.PrefixElementNamespace);
                var sequenceIn= doc.CreateElement(document.PrefixElement, "sequence", document.PrefixElementNamespace);
                foreach (var r in s.GetParameters()) {
                    var elementParams = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                    elementParams.SetAttribute("minOccurs","0");
                    elementParams.SetAttribute("maxOccurs","1");
                    elementParams.SetAttribute("name",r.Name);
                    elementParams.SetAttribute("type",document.PrefixElement+":"+TypeConverter.FormDoNet(r.ParameterType));
                    sequenceIn.AppendChild(elementParams);
                }
                complexTypeIn.AppendChild(sequenceIn);
                elementIn.AppendChild(complexTypeIn);
                schema.AppendChild(elementIn);

                //return type
                var elementOut = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                elementOut.SetAttribute("name",s.Name+"Response");
                var complexTypeOut = doc.CreateElement(document.PrefixElement, "complexType", document.PrefixElementNamespace);
                var sequenceOut = doc.CreateElement(document.PrefixElement, "sequence", document.PrefixElementNamespace);
                var returnType = s.ReturnType;

                var elementReturn = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                elementReturn.SetAttribute("minOccurs", "0");
                elementReturn.SetAttribute("maxOccurs", "1");
                elementReturn.SetAttribute("name", returnType.Name);
                elementReturn.SetAttribute("type", document.PrefixElement + ":" + TypeConverter.FormDoNet(returnType.UnderlyingSystemType));

                sequenceOut.AppendChild(elementReturn);
                complexTypeOut.AppendChild(sequenceOut);
                elementOut.AppendChild(complexTypeOut);
                schema.AppendChild(elementOut);
            }

            //messages
            string[] soapActions = new string[] { "SoapIn", "SoapOut", "HttpGetIn", "HttpGetOut", "HttpPostIn", "HttpPostOut" };
            foreach (var s in document.ApiTypes.Methods)
            {
                foreach (var act in soapActions)
                {
                    if (!document.AllowHttpAccess && act.Contains("Http"))
                    {
                        continue;
                    }

                    var messageIn = doc.CreateElement(document.Prefix, "message", document.WSDLNamespace);
                    messageIn.SetAttribute("name", s.Name + act);

                    if (act == "SoapIn" || act == "SoapOut")
                    {
                        var partIn = doc.CreateElement(document.Prefix, "part", document.WSDLNamespace);
                        partIn.SetAttribute("name", "parameters");
                        if (act == "SoapIn")
                        {
                            partIn.SetAttribute("element", "tns:" + s.Name);
                        }else
                        {
                            partIn.SetAttribute("element", "tns:" + s.Name+"Response");
                        }
                        messageIn.AppendChild(partIn);
                    }
                    else if (act == "HttpGetIn" || act == "HttpPostIn")
                    {
                        foreach (var r in s.GetParameters())
                        {
                            var partParam = doc.CreateElement(document.Prefix, "part", document.WSDLNamespace);
                            partParam.SetAttribute("name", r.Name);
                            partParam.SetAttribute("type", document.Prefix + ":" + TypeConverter.FormDoNet(r.ParameterType));
                            messageIn.AppendChild(partParam);
                        }
                    }
                    else
                    {
                        //TODO 未完成
                    }
                    definitions.AppendChild(messageIn);
                }
            }
            //portTypes
            string[] portTypes = new string[] { "UserSoap","UserHttpGet","UserHttpPost" };
            foreach (var p in portTypes)
            {                                        
                var portType = doc.CreateElement(document.Prefix, "portType", document.WSDLNamespace);
                portType.SetAttribute("name", p);                
                if (!document.AllowHttpAccess && p.Contains("Http"))
                {
                    continue;
                }
                foreach (var s in document.ApiTypes.Methods)
                {
                    var operation = doc.CreateElement(document.Prefix, "operation", document.WSDLNamespace);
                    operation.SetAttribute("name", s.Name);
                    var documentationPorttType = doc.CreateElement(document.Prefix, "documentation", document.WSDLNamespace);
                    var input = doc.CreateElement(document.Prefix, "input", document.WSDLNamespace);
                    input.SetAttribute("message", "tns:" + s.Name + p + "In");
                    var output = doc.CreateElement(document.Prefix, "output", document.WSDLNamespace);
                    output.SetAttribute("message", "tns" + s.Name + p + "Out");
                    operation.AppendChild(documentationPorttType);
                    operation.AppendChild(input);
                    operation.AppendChild(output);
                    portType.AppendChild(operation);
                    definitions.AppendChild(portType);
                }
            }
            //binding
            string[] bindingTypes = new string[] { "UserSoap", "UserSoap12", "UserHttpGet", "UserHttpPost" };
            foreach (var p in bindingTypes)
            {
                if (!document.AllowHttpAccess && p.Contains("Http"))
                {
                    continue;
                }

                var binding = doc.CreateElement(document.Prefix, "binding", document.WSDLNamespace);
                binding.SetAttribute("name", p);
                binding.SetAttribute("type", "tns:" + p);

                var soapBinding = doc.CreateElement("soap", "binding", document.NS_soap);
                soapBinding.SetAttribute("transport", document.NS_soap);
                binding.AppendChild(soapBinding);

                foreach (var r in document.ApiTypes.Methods)
                { 

                    var operation = doc.CreateElement(document.Prefix, "operation", document.WSDLNamespace);
                    operation.SetAttribute("name", r.Name);
                    var soapOperation = doc.CreateElement("soap", "operation", document.NS_soap);
                    soapOperation.SetAttribute("soapAction", document.Namespace+"/"+r.Name);
                    soapOperation.SetAttribute("style", "document");

                    //input 
                    var input = doc.CreateElement(document.Prefix, "input", document.WSDLNamespace);                    
                    var bodyInput = doc.CreateElement("soap", "body", document.NS_soap);
                    bodyInput.SetAttribute("use", "literal");
                    input.AppendChild(bodyInput);

                    //output
                    var output = doc.CreateElement(document.Prefix, "output", document.WSDLNamespace);
                    var bodyOutput = doc.CreateElement("soap", "body", document.NS_soap);
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
            var service = doc.CreateElement(document.Prefix, "service", document.WSDLNamespace);
            service.SetAttribute("name", document.Name);
            var serviceDocumentation =doc.CreateElement(document.Prefix, "documentation", document.WSDLNamespace);
            serviceDocumentation.InnerText = document.Description;
            service.AppendChild(serviceDocumentation);
            foreach (var p in bindingTypes)
            {
                var port = doc.CreateElement(document.Prefix, "port", document.WSDLNamespace);
                port.SetAttribute("name", p);
                port.SetAttribute("binding", "tns:" + p);
                var address = doc.CreateElement("soap", "address", document.NS_soap);
                address.SetAttribute("location", document.ServiceAddress);
                port.AppendChild(address);
                service.AppendChild(port);
            }
            definitions.AppendChild(service);
            return doc.OuterXml;

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
}
