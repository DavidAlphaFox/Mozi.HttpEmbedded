using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozi.HttpEmbedded.Page;

namespace Mozi.HttpEmbedded.Test
{
    public class Test:BaseApi
    {
        public string Guid()
        {
            return Mozi.SSDP.UUID.Generate();
        }
    }
}
