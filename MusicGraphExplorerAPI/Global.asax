<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.ServiceModel.Activation" %>
<%@ Import Namespace="System.ServiceModel.Web " %>

<script RunAt="server">
    void Application_Start(object sender, EventArgs e)
    {
        RegisterRoutes(RouteTable.Routes);
    }

    private void RegisterRoutes(RouteCollection routes)
    {
        routes.Add(new ServiceRoute("api", new WebServiceHostFactory(), typeof(MusicGraphExplorerAPI.MusicGraphAPI))); 
   }
</script>