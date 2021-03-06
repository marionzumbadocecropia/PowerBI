﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace PowerBI.Models
{
    public class PowerbiSettings
    {
        public string UserName
        {
            get { return ConfigurationManager.AppSettings["pbiUsername"]; }
            set { ConfigurationManager.AppSettings["pbiUserName"] = value; }
        }

        public string Password
        {
            get { return ConfigurationManager.AppSettings["pbiPassword"]; }
            set { ConfigurationManager.AppSettings["pbiPassword"] = value; }
        }

        public string AuthorityUrl
        {
            get { return ConfigurationManager.AppSettings["authorityUrl"]; }
            set { ConfigurationManager.AppSettings["authorityUrl"] = value; }
        }

        public string ResourceUrl
        {
            get { return ConfigurationManager.AppSettings["resourceUrl"]; }
            set { ConfigurationManager.AppSettings["resourceUrl"] = value; }
        }

        public string ApiUrl
        {
            get { return ConfigurationManager.AppSettings["apiUrl"]; }
            set { ConfigurationManager.AppSettings["apiUrl"] = value; }
        }

        public string EmbedUrlBase
        {
            get { return ConfigurationManager.AppSettings["embedUrlBase"]; }
            set { ConfigurationManager.AppSettings["embedUrlBase"] = value; }
        }

        public string ApplicationId
        {
            get { return ConfigurationManager.AppSettings["applicationId"]; }
            set { ConfigurationManager.AppSettings["applicationId"] = value; }
        }

        public string WorkspaceId
        {
            get { return ConfigurationManager.AppSettings["workspaceId"]; }
            set { ConfigurationManager.AppSettings["workspaceId"] = value; }
        }

        public string ReportId
        {
            get { return ConfigurationManager.AppSettings["reportId"]; }
            set { ConfigurationManager.AppSettings["reportId"] = value; }
        }


    }
}