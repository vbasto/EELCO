using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.IO;

namespace PickDispatch
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
    public class pickdispatchservice : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetCustomers()
        {
            return WService.SqlToJSON("select group_id,[name] from EELCOGAV.dbo.CAT_CustomerGroup order by [name]");
        }

        [WebMethod]
        public string GetInventoryFromCustomer(string customer_id)
        {
            return WService.SqlToJSON("select L.OID as 'LPID', R.WhseReceiptNo as 'ReceiptNo', convert(varchar(10), L.CreatedAt, 101) as 'Date', B.TrailerNo as 'Equipment', I.CustomerOrderNo as 'OrderNo', B.BLNo as 'BLNo', I.InvoiceNo as 'InvoiceNo', L.GrossWeight as 'GrossWeight'  " +
                                      "from WhseReceiptLPN L join WhseReceiptBL B on (B.OID = L.BL) join WhseReceipt R on (R.OID = L.WReceiptRel) join DocBase D on (D.OID = R.OID) join Traffic T on (T.OID = D.Traffic) join WhseReceiptRItem I on (I.LPN = L.OID) " +
                                      "where 1 = 1 and T.Customer in(select customer_id from EELCOGAV.dbo.txCustomerGroup where group_id = " + customer_id + ") and L.WDispatchRel is null order by R.WhseReceiptNo desc, 1");
        }

        [WebMethod]
        public string GetHeaderInfo(string serial)
        {
            return WService.SqlToJSON("select top 1 C.name as 'CustomerName', E.name as 'ConsigneeName', B.BLNo, B.Packages as 'Packages', B.GrossWeight as 'GrossWeight', I.CustomerOrderNo as 'CustomerOrder', replace(A.street,',','') + ',' + isnull(A.City,'') + ' ' + isnull(S.Code,'') + ' ' + isnull(A.ZipCode,'') as 'ConsigneeAddress' " +
                                      ", E.OID as 'ConsigneeID', R.NotifyParty as 'NotifyPartyId', EBNP.Name as 'NotifyPartyName', E.name as 'FreightPartyName', E.OID as 'FreightPartyId', C.OID as 'CustomerId' " +
                                      ", (select rtrim(FirstName) + ' ' + rtrim(LastName) from Person where OID in(select CreatedBy from WhseReceiptLog where WReceiptRel = R.OID and NewStatus = 20)) as 'ReceivedBy' " +
                                      ", (select CreatedAt from WhseReceiptLog where WReceiptRel = R.OID and NewStatus = 20) as 'ReceivedBy' " +
                                      "from WhseReceiptLPN L join WhseReceipt R on (L.WReceiptRel = R.OID) join WhseReceiptGood G on (G.WhseReceipt = R.OID) join DocBase D on (D.OID = R.OID) join Traffic T on (T.OID = D.Traffic) join Customer C on (T.Customer = C.OID) join WhseConsignee WC on (G.RelWhseConsignee = WC.OID) " +
                                      "join EntityBase E on (WC.OID = E.OID)join WhseReceiptBL B on (B.Receipt = R.OID) join EntityAddress A on (A.OID = E.Address) join WhseReceiptRItem I on (I.LPN = L.OID) left join State S on (S.OID = A.State) left join EntityBase EBNP on (EBNP.OID = R.NotifyParty) " +
                                      "where L.OID='" + serial + "'");
        }

        [WebMethod]
        public string GetNotifyParty()
        {
            return WService.SqlToJSON("select OID as 'Value', Name as 'Name' from EntityBase where OID in(select OID from WhseNotifyParty) order by Name");
        }

        [WebMethod]
        public string GetFreightParty()
        {
            return WService.SqlToJSON("select OID as 'Value', Name as 'Name' from EntityBase where OID in(select OID from WhseConsignee) and Enabled=1 and Name is not null order by Name");
        }

        [WebMethod]
        public string ProcessPickDispatch(string carrierName, string consigneeId, string bol, string remarks, string customerOrders, string packages, string notifyParty, string trackingNo, string dispatchedBy, string consigneeAddress, string consigneeAddress2, string freightParty, string seal, string grossWeight, string driverName, string loadNo, string customerId, string trafficNo, string orderNo, string notifyPartyName)
        {
            return WService.SqlToJSON("declare @nextPickNo varchar(10), @nextDispatchNo varchar(10), @trafficId int, @docbaseId int, @docbaseDispatchId int; " +
                                      "set @nextPickNo = (select top 1 right('0000000000' + cast(cast(WhsePickNo as int) + 1 as varchar(10)), 10) as 'NextPickNo' from WhsePick order by WhsePickNo desc); " +
                                      "set @nextDispatchNo = (select top 1 right('0000000000' + cast(cast(WhseDispatchNo as int) + 1 as varchar(10)), 10) as 'NextDispatchNo' from WhseDispatch order by WhseDispatchNo desc); " +
                                      "insert into Traffic ([CreatedAt],[CreatedBy],[TrafficNo],[Customer],[TrafficDate],[Carrier],[Status],[Reference],[OptimisticLockField],[Equipment],[DocsRecDate],[OfficeRel],[InternalNotes],[ShipmentNo],[PendingInformation]) values(getdate(),3733, '" + trafficNo + "', " + customerId + ", getdate(),null,0,'',0,NULL,NULL,NULL,NULL,NULL,NULL); select @trafficId=SCOPE_IDENTITY(); " +
                                      "insert into DocBase ([CreatedAt],[CreatedBy],[Traffic],[OptimisticLockField],[ObjectType],[Penalty],[PostAudit],[Mitigation],[CorrectiveAction],[PostIssue],[OfficeRel]) values(getdate(),3733, @trafficId, 4,266,0,0,0,NULL,NULL,NULL); select @docbaseId=SCOPE_IDENTITY(); " +
                                      "insert into WhsePick values(@docbaseId, 0, '" + carrierName + "', " + consigneeId + ", '" + bol + "', NULL, '" + remarks + "', NULL, @nextPickNo, 1, '" + customerOrders + "', null, 0, 1, getdate(), null, 0, " + packages + ", " + notifyParty + ", null, null, null, null); " +
                                      "insert into DocBase ([CreatedAt],[CreatedBy],[Traffic],[OptimisticLockField],[ObjectType],[Penalty],[PostAudit],[Mitigation],[CorrectiveAction],[PostIssue],[OfficeRel]) values(getdate(),3733, @trafficId, 4,126,0,0,0,NULL,NULL,NULL); select @docbaseDispatchId=SCOPE_IDENTITY(); " +
                                      "insert into WhseDispatch ([OID],[WhseDispatchNo],[WhseId],[DispatchedDate],[Carrier],[Consignee],[VehicleNo],[DriverName],[BOL],[DispatchedBy],[CustomerReference],[Delivered],[DispatchNotes],[ConsAddress1],[ConsAddress2],[ConsContact],[BOLChargeType],[FreightParty],[CarrierName],[CustAlias],[Seal],[DailyNumber],[CustomerAdditionalRef],[CBPRelDoc],[AppointmentDate],[Halt],[GrossWt],[WeightUnit],[NotifyParty],[BOLDescription],[StartDispatch]) " + 
                                      "values(@docbaseDispatchId, @nextDispatchNo, 1, getdate(), null, " + consigneeId + ", '" + trackingNo + "', '" + driverName + "', '" + bol + "', " + dispatchedBy + ", '" + customerOrders + "', 0, '" + remarks + "', '" + consigneeAddress + "', '"+ consigneeAddress2 + "', '', null, " + consigneeId + ", '" + carrierName + "', '" + notifyPartyName + "', '" + seal + "', " + orderNo + ", '" + loadNo + "', null, getdate(), 0, " + grossWeight + ", 0, " + notifyParty +", null, getdate()); " +
                                      "update WhsePick set WDispRelated=@docbaseDispatchId where OID=@docbaseId;" +
                                      "select @nextPickNo as 'PickNo', @nextDispatchNo as 'DispatchNo', @docbaseId as 'PickId', @docbaseDispatchId as 'DispatchId';");
        }

        [WebMethod]
        public string UpdateLPN(string lp, string pickId, string dispatchId)
        {
            return WService.SqlToJSON("update WhseReceiptLPN set WDispatchRel = " + dispatchId + ", WPickRelated=" + pickId + ", DispatchedOn=getdate() where OID = " + lp + "; " +
                                      "select 'OK'");
        }
    }
}
