using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.PowerBI.Api.V2.Models;

namespace PowerBI.Models
{
    [Serializable]//to serializa the object to the client
    public class EmbedConfig
    {
        public string Id { get; set; }
        public string EmbedUrl { get; set; }//for organizational uses if they want to directly embed an object
        public EmbedToken EmbedToken { get; set; }//houses the actual embed token
        public string ErrorMessage { get; internal set; }
    }
}