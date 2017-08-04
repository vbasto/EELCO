using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Reporteador
{
    public partial class index : System.Web.UI.Page
    {
        string columns = "R.WhseReceiptNo as 'ReceiptNo' " +
                           ", R.ReceivedDate as 'ReceiptDate' " +
                           ", W.Name as 'WarehouseName' " +
                           ", EA.Street as 'WarehouseAddress' " +
                           ", EA.City as 'WarehouseCity' " +
                           ", ST.Code as 'WarehouseStateCode' " +
                           ", ST.Name as 'WarehouseStateName' " +
                           ", CB.Code as 'CarrierSCAC' " +
                           ", CB.Name as 'CarrierName' " +
                           ", C.Add1 as 'CarrierAddress1' " +
                           ", C.Add2 as 'CarrierAddress2' " +
                           ", EBS.KeyNo as 'ShipperKey' " +
                           ", EBS.Name as 'ShipperName' " +
                           ", R.VehicleNo as 'VehicleNo' " +
                           ", R.DeliveredBy as 'DeliveredBy' " +
                           ", R.BOL as 'BOL' " +
                           ", E.Login as 'ReceivedBy' " +
                           ", E.Email as 'ReceivedByEmail' " +
                           ", R.CustomerReference as 'CustomerReference' " +
                           ", R.ReceiptComments as 'ReceiptComments' " +
                           ", R.EquipmentNo as 'EquipmentNo' " +
                           ", R.EquipmentProvider as 'EquipmentProvider' " +
                           ", case when R.RStatus = '0' then 'ASN' " +
                                 "when R.RStatus = '10' then 'IN ROUTE' " +
                                 "when R.RStatus = '20' then 'SCHEDULED' " +
                                 "when R.RStatus = '25' then 'AT YARD' " +
                                 "when R.RStatus = '30' then 'ARRIVED' " +
                                 "when R.RStatus = '35' then 'UNLOADING' " +
                                 "when R.RStatus = '40' then 'RECEIVED' " +
                                 "when R.RStatus = '45' then 'PICKED' " +
                                 "when R.RStatus = '50' then 'SHIPPED' " +
                                 "when R.RStatus = '60' then 'CANCELLED' " +
                                 "end as 'ReceiptStatus' " +
                           ", BL.CreatedAt as 'BLCreatedAt' " +
                           ", BL.BLNo as 'BLNo' " +
                           ", EBL.Login as 'BLCreatedBy' " +
                           ", BL.TrailerNo as 'BLTrailerNo' " +
                           ", BL.GrossWeight as 'BLGrossWeight' " +
                           ", BL.VehicleNo as 'BLVehicleNo' " +
                           ", BL.EquipmentProvider as 'BLEquipmentProvider' " +
                           ", BL.Door as 'BLDoor' " +
                           ", BL.WeightUnit as 'BLWeightUnit' " +
                           ", BL.DeliveredBy as 'BLDeliveredBy' " +
                           ", EBLR.Login as 'BLReceivedBy' " +
                           ", BL.ReceiptComments as 'BLReceiptComments' " +
                           ", BL.Packages as 'BLPackages' " +
                           ", BL.Damages as 'BLDamages' " +
                           ", BL.ReceivedOn as 'BLReceivedOn' " +
                           ", BL.PlatesPrinted as 'BLPlatesPrinted' " +
                           ", BL.Revision as 'BLRevision' " +
                           ", BL.EntryRef as 'BLEntryRef' " +
                           ", BL.DriverName as 'BLDriverName' " +
                           ", BL.FinalCBPWithdrawDt as 'BLFinalCBPWithdrawDt' " +
                           ", BL.CBPClear as 'BLCBPClear' " +
                           ", BL.ImportDt as 'BLImportDt' " +
                           ", BL.PermitExpirationDt as 'BLPermitExpirationDt' " +
                           ", BL.Appointment as 'BLAppointment' " +
                           ", BL.Instructions as 'BLInstructions' " +
                           ", BL.EquipmentSeal as 'BLEquipmentSeal' " +
                           ", BL.StartReceiving as 'BLStartReceiving' " +
                           ", EBLP.Login as 'BLPutawayBy' ";

        string queryReceipts = "from WhseReceipt R " +
                               "left join Warehouse W on (W.OID = R.WhseId) " +
                               "left join Carrier C on (C.OID = R.Carrier) " +
                               "left join CarrierBase CB on (CB.OID = C.OID) " +
                               "left join Shipper S on (S.OID = R.ShipBy) " +
                               "left join EntityBase EBS on (EBS.OID = S.OID) " +
                               "left join Employee E on (E.OID = R.ReceivedBy) " +
                               "left join EntityAddress EA on (EA.OID = W.WhseAddress) " +
                               "left join State ST on (ST.OID = EA.State) " +
                               "left join WhseReceiptBL BL on (BL.Receipt = R.OID) " +
                               "left join Employee EBL on (EBL.OID = BL.CreatedBy) " +
                               "left join Employee EBLR on (EBLR.OID = BL.ReceivedBy) " +
                               "left join Employee EBLP on (EBLP.OID = BL.PutawayBy) " +
                               "order by R.WhseReceiptNo";

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btReceipts_Click(object sender, EventArgs e)
        {
            string delimiter = " as ";
            string[] aux = columns.Split(',');
            foreach(string i in aux)
            {
                string[] aux2 = i.Split(new[] { delimiter }, StringSplitOptions.None);
                string columnName = aux2[1].Trim().Replace("'","");
                ListItem li = new ListItem(columnName, i);
                cblColumns.Items.Add(li);
            }
        }

        protected void btPreview_Click(object sender, EventArgs e)
        {
            string dynamicQuery = "select ";
            foreach(ListItem l in cblColumns.Items)
            {
                if (l.Selected) { dynamicQuery += l.Value + ","; }
            }
            dynamicQuery = dynamicQuery.Substring(0, dynamicQuery.Length - 1);
            dynamicQuery += queryReceipts;
        }
    }
}