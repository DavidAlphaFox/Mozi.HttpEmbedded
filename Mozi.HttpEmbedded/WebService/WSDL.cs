using System.Collections.Generic;
using System.IO;
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

        public string PrefixElement = "xs";
        public string PrefixElementNamespace = "http://www.w3.org/2001/XMLSchema";

        public string Name = "Mozi.WebService服务信息";

        public string ServiceAddress = "http://127.0.0.1/runtime/soap?action=wsdl";
          
        public Types ApiTypes { get; set; }

        public WSDL()
        {
          
            ApiTypes = new Types();
        }

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
            documentation.InnerText=document.Name;
            definitions.AppendChild(documentation);
            //types
            var types= doc.CreateElement(document.Prefix, "types",null);
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
                var sequence= doc.CreateElement(document.PrefixElement, "sequence", document.PrefixElementNamespace);
                foreach (var r in s.GetParameters()) {
                    var elementParams = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                    elementParams.SetAttribute("minOccurs","0");
                    elementParams.SetAttribute("maxOccurs","1");
                    elementParams.SetAttribute("name",r.Name);
                    elementParams.SetAttribute("type", r.GetType().ToString());
                    sequence.AppendChild(elementParams);
                }
                complexTypeIn.AppendChild(sequence);
                elementIn.AppendChild(complexTypeIn);
                schema.AppendChild(elementIn);

                //return type
                var elementOut = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                elementOut.SetAttribute("name",s.Name+"Response");
                var complexTypeOut = doc.CreateElement(document.PrefixElement, "complexType", document.PrefixElementNamespace);
                var sequenceOut = doc.CreateElement(document.PrefixElement, "sequence", document.PrefixElementNamespace);
                foreach (var r in s.GetParameters())
                {
                    var elementParams = doc.CreateElement(document.PrefixElement, "element", document.PrefixElementNamespace);
                    elementParams.SetAttribute("minOccurs", "0");
                    elementParams.SetAttribute("maxOccurs", "1");
                    elementParams.SetAttribute("name", r.Name);
                    elementParams.SetAttribute("type", r.GetType().ToString());
                    sequence.AppendChild(elementParams);
                }
                complexTypeIn.AppendChild(sequenceOut);
                elementIn.AppendChild(complexTypeOut);
                schema.AppendChild(elementOut);
            }

            //messages
            string[] soapActions = new string[] { "SoapIn", "SoapOut", "HttpGetIn", "HttpGetIn", "HttpPostIn", "HttpPostOut" };
            foreach (var act in soapActions)
            {
                foreach (var s in document.ApiTypes.Methods)
                {
                    var messageIn = doc.CreateElement(document.PrefixElement, "message", document.PrefixElementNamespace);
                    messageIn.SetAttribute("name", s.Name + act);
                    var partIn = doc.CreateElement(document.PrefixElement, "part", document.PrefixElementNamespace);
                    partIn.SetAttribute("name", "parameters");
                    if(act=="SoapIn"||act=="SoapOut")
                    {
                        partIn.SetAttribute("element", "tns:" + s.Name);
                    }
                    messageIn.AppendChild(partIn);
                    definitions.AppendChild(messageIn);
                }
            }
            //portTypes
            string[] portTypes = new string[] { "UserSoap","UserHttpGet","UserHttpPost" };
            foreach(var p in portTypes)
            {
                var portType = doc.CreateElement(document.PrefixElement, "portType", document.PrefixElementNamespace);
                portType.SetAttribute("name", p);
                foreach (var s in document.ApiTypes.Methods)
                {
                    var operation = doc.CreateElement(document.PrefixElement, "operation", document.PrefixElementNamespace);
                    operation.SetAttribute("name", s.Name );
                    var documentationPorttType = doc.CreateElement(document.PrefixElement, "documentation", document.PrefixElementNamespace);
                    var input= doc.CreateElement(document.PrefixElement, "input", document.PrefixElementNamespace);
                    input.SetAttribute("message", "tns:" + s.Name + p + "In");
                    var output = doc.CreateElement(document.PrefixElement, "output", document.PrefixElementNamespace);
                    output.SetAttribute("message", "tns" + s.Name + p + "Out");
                    operation.AppendChild(documentationPorttType);
                    operation.AppendChild(input);
                    operation.AppendChild(output);
                    portType.AppendChild(operation);
                }
                definitions.AppendChild(portType);
            }
            //binding
            string[] bindingTypes = new string[] { "UserSoap", "UserSoap12", "UserHttpGet", "UserHttpPost" };
            foreach (var p in bindingTypes)
            {
                var binding = doc.CreateElement(document.PrefixElement, "binding", document.PrefixElementNamespace);
                binding.SetAttribute("name", p);
                binding.SetAttribute("type", "tns:" + p);

                var soapBinding = doc.CreateElement("soap", "binding", document.NS_soap);
                soapBinding.SetAttribute("transport", document.NS_soap);
                binding.AppendChild(soapBinding);

                foreach (var r in document.ApiTypes.Methods)
                { 
                    var operation = doc.CreateElement(document.PrefixElement, "operation", document.PrefixElementNamespace);
                    operation.SetAttribute("name", r.Name);
                    var soapOperation = doc.CreateElement("soap", "operation", document.NS_soap);
                    soapOperation.SetAttribute("soapAction", document.Namespace+"/"+r.Name);
                    soapOperation.SetAttribute("style", "document");

                    //input 
                    var input = doc.CreateElement(document.PrefixElement, "input", document.PrefixElementNamespace);                    
                    var bodyInput = doc.CreateElement("soap", "body", document.NS_soap);
                    bodyInput.SetAttribute("use", "literal");
                    input.AppendChild(bodyInput);

                    //output
                    var output = doc.CreateElement(document.PrefixElement, "output", document.PrefixElementNamespace);
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
            var service = doc.CreateElement(document.PrefixElement, "service", document.PrefixElementNamespace);
            var serviceDocumentation =doc.CreateElement(document.PrefixElement, "documentation", document.PrefixElementNamespace);
            serviceDocumentation.InnerText = document.Name;
            service.AppendChild(serviceDocumentation);
            foreach (var p in bindingTypes)
            {
                var port = doc.CreateElement(document.PrefixElement, "port", document.PrefixElementNamespace);
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
}
