using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory; //to setup user credentials, an authentication context and get a token
using System.Threading.Tasks;
using Microsoft.Rest;
using PowerBI.Models;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using System.Web.Http.Cors;

namespace PowerBI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class HomeController : Controller
    {
        private PowerbiSettings powerbiSettings = new PowerbiSettings();
        private async Task<AuthenticationResult> Authenticate()
        {
            //create a user password credentials
            var credential = new UserPasswordCredential(powerbiSettings.UserName, powerbiSettings.Password);
            //stablish an authenticatoon contex, authenticate using created credentials
            var authenticationContext = new AuthenticationContext(powerbiSettings.AuthorityUrl);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(powerbiSettings.ResourceUrl, powerbiSettings.ApplicationId, credential); //this var will have the token
            return authenticationResult;
        }

        private async Task<TokenCredentials> CreateCredentials()
        {
            AuthenticationResult authenticationResult = await Authenticate();
            if(authenticationResult == null){
                return null;//no success
            }
            TokenCredentials tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            return tokenCredentials;
        }

        public async Task<ActionResult> GetReports(string workspaceId)
        {
            if (string.IsNullOrWhiteSpace(workspaceId))
            {

                workspaceId = powerbiSettings.WorkspaceId;
            }
            try
            {
                TokenCredentials tokenCredentials = await CreateCredentials();
                if (tokenCredentials == null)
                {
                    var error = "Authentication Failed";
                    return Json(error, JsonRequestBehavior.AllowGet);
                }
                using(var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
                {
                    var reports = await client.Reports.GetReportsInGroupAsync(workspaceId);
                    return Json(reports.Value, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EmbedReport(string reportId, string workspaceId)
        {
            if (string.IsNullOrWhiteSpace(reportId))
            {

                reportId = powerbiSettings.ReportId;
            }
            if (string.IsNullOrWhiteSpace(workspaceId))
            {

                workspaceId = powerbiSettings.WorkspaceId;
            }
            var result = new EmbedConfig();
            try
            {
                TokenCredentials tokenCredentials = await CreateCredentials();
                if (tokenCredentials == null)
                {
                    var error = "Authentication Failed";
                    return Json(error, JsonRequestBehavior.AllowGet);
                }
                using (var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
                {
                    GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

                    Report report = await client.Reports.GetReportInGroupAsync(workspaceId, reportId);

                    var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(workspaceId, report.Id, generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        result.ErrorMessage = "Failed to generate emded token";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    result.EmbedToken = tokenResponse;
                    result.EmbedUrl = report.EmbedUrl;
                    result.Id = report.Id;

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch(Exception ex)
            {
                result.ErrorMessage = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }  

        }

        public async Task<ActionResult> EmbedDashboard(string dashboardId, string workspaceId)
        {
            if (string.IsNullOrWhiteSpace(workspaceId))
            {

                workspaceId = powerbiSettings.WorkspaceId;
            }
            TokenCredentials tokenCredentials = await CreateCredentials();
            if (tokenCredentials == null)
            {
                var error = "Authentication Failed";
                return Json(error, JsonRequestBehavior.AllowGet);
            }

            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(workspaceId);

                // Get the first report in the workspace.
                var dashboard = dashboards.Value.Where(x => x.Id == dashboardId).FirstOrDefault();

                if (dashboard == null)
                {
                    return Json(new EmbedConfig()
                    {
                        ErrorMessage = "Workspace has no dashboards."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Generate Embed Token.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Dashboards.GenerateTokenInGroupAsync(powerbiSettings.WorkspaceId, dashboard.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    return Json(new EmbedConfig()
                    {
                        ErrorMessage = "Failed to generate embed token."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Generate Embed Configuration.
                var embedConfig = new EmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = dashboard.EmbedUrl,
                    Id = dashboard.Id
                };

                return Json(embedConfig, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> EmbedTile(string tileId, string dashboardId)
        {
            TokenCredentials tokenCredentials = await CreateCredentials();
            if (tokenCredentials == null)
            {
                var error = "Authentication Failed";
                return Json(error, JsonRequestBehavior.AllowGet);
            }

            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(powerbiSettings.WorkspaceId);

                // Get the first report in the workspace.
                var dashboard = dashboards.Value.Where(x => x.Id == dashboardId).FirstOrDefault();

                if (dashboard == null)
                {
                    return Json(new TileEmbedConfig()
                    {
                        ErrorMessage = "Workspace has no dashboards."
                    }, JsonRequestBehavior.AllowGet);
                }

                var tiles = await client.Dashboards.GetTilesInGroupAsync(powerbiSettings.WorkspaceId, dashboard.Id);

                // Get the first tile in the workspace.
                var tile = tiles.Value.Where(x=> x.Id == tileId).FirstOrDefault();

                // Generate Embed Token for a tile.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Tiles.GenerateTokenInGroupAsync(powerbiSettings.WorkspaceId, dashboard.Id, tile.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    return Json(new TileEmbedConfig()
                    {
                        ErrorMessage = "Failed to generate embed token."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Generate Embed Configuration.
                var embedConfig = new TileEmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = tile.EmbedUrl,
                    Id = tile.Id,
                    dashboardId = dashboard.Id
                };

                return Json(embedConfig, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetDashboards(string username, string roles)
        {
            var result = new EmbedConfig();
            try
            {
                TokenCredentials tokenCredentials = await CreateCredentials();
                if (tokenCredentials == null)
                {
                    var error = "Authentication Failed";
                    return Json(error, JsonRequestBehavior.AllowGet);
                }

                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
                {
                    // Get a list of dashboards.
                    var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(powerbiSettings.WorkspaceId);
                    return Json(dashboards.Value, JsonRequestBehavior.AllowGet);
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }
            return Json(result,JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetTiles(string username, string roles)
        {
            var result = new EmbedConfig();
            try
            {
                TokenCredentials tokenCredentials = await CreateCredentials();
                if (tokenCredentials == null)
                {
                    var error = "Authentication Failed";
                    return Json(error, JsonRequestBehavior.AllowGet);
                }

                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(powerbiSettings.ApiUrl), tokenCredentials))
                {
                    List<Tile> tiles = new List<Tile>();
                    // Get a list of dashboards.
                    var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(powerbiSettings.WorkspaceId);
                    for (int i = 0; i < dashboards.Value.Count; i++)
                    {
                        var tilesTemp = await client.Dashboards.GetTilesInGroupAsync(powerbiSettings.WorkspaceId, dashboards.Value[i].Id);
                        for(int i2=0; i2 < tilesTemp.Value.Count; i2++)
                        {
                            tilesTemp.Value[i2].ReportId = dashboards.Value[i].Id;//TO DO: Change to return dashboard id
                            tiles.Add(tilesTemp.Value[i2]);
                        }
                    }
                    return Json(tiles, JsonRequestBehavior.AllowGet);
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check if web.config embed parameters have valid values.
        /// </summary>
        /// <returns>Null if web.config parameters are valid, otherwise returns specific error string.</returns>
        private string GetWebConfigErrors()
        {
            // Application Id must have a value.
            if (string.IsNullOrWhiteSpace(powerbiSettings.ApplicationId))
            {
                return "ApplicationId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Application Id must be a Guid object.
            Guid result;
            if (!Guid.TryParse(powerbiSettings.ApplicationId, out result))
            {
                return "ApplicationId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill application Id in web.config.";
            }

            // Workspace Id must have a value.
            if (string.IsNullOrWhiteSpace(powerbiSettings.WorkspaceId))
            {
                return "WorkspaceId is empty. Please select a group you own and fill its Id in web.config";
            }

            // Workspace Id must be a Guid object.
            if (!Guid.TryParse(powerbiSettings.WorkspaceId, out result))
            {
                return "WorkspaceId must be a Guid object. Please select a workspace you own and fill its Id in web.config";
            }

            // Username must have a value.
            if (string.IsNullOrWhiteSpace(powerbiSettings.UserName))
            {
                return "Username is empty. Please fill Power BI username in web.config";
            }

            // Password must have a value.
            if (string.IsNullOrWhiteSpace(powerbiSettings.Password))
            {
                return "Password is empty. Please fill password of Power BI username in web.config";
            }

            return null;
        }
    }
}
