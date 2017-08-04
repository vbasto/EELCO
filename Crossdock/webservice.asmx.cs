using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.IO;

namespace Crossdock
{
    public static class WService
    {
        public static SqlConnection CreateAppConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
        }

        public static string DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(list);
        }


        public static string SqlToJSON(string query)
        {
            using (var conex = WService.CreateAppConnection())
            using (var adapter = new SqlDataAdapter(query, conex))
            using (var table = new DataTable())
            {
                adapter.Fill(table);
                return DataTableToJSON(table);
            }
        }

        public static string SqlExec(string query, Dictionary<string, string> parameters)
        {
            using (var conex = WService.CreateAppConnection())
            using (var command = new SqlCommand(query, conex))
            {
                foreach (var item in parameters)
                    command.Parameters.AddWithValue("@" + item.Key, item.Value);

                conex.Open();
                var res = command.ExecuteScalar();
                return (res != null) ? res.ToString() : "";
            }
        }
    }

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class webservice : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetActiveShipments()
        {
            return WService.SqlToJSON("EXEC GetActiveShipments");
        }

        [WebMethod]
        public string GetDispatchedShipments()
        {
            return WService.SqlToJSON("EXEC GetDispatchedShipments");
        }
    }
}
