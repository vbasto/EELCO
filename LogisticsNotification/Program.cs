using System;
using System.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;


/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 *  DEVELOPED BY: 
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  GAV TECHNOLOGIES (VICTOR BASTO)
 * 
 * 
 *  FOR: 
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  EELCO SUPPLY CHAIN SOLUTIONS
 *  WAREHOUSE DEPARTMENT
 * 
 * 
 *  PROBLEM:
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  EVERY TIME THE WAREHOUSE RECEIVES A LOADED TRUCK, THEY NEED TO SEND AN EMAIL TO THE LOGISTIC ANNOUNCING THE ARRIVAL AND GIVING INSTRUCTIONS FOR THE PICKUP.
 *  LOGISTIC CATALOG ARE MANAGED IN EXCEL DOCUMENTS.
 *  LOGISTIC CONTACTS ARE SAVED IN OLD EMAILS.
 *  
 *  
 *  SOLUTION:
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  1) DESIGN A CUSTOM DATABASE TO MANAGE THE LOGISTIC CONTACTS AND SAVE A HISTORY OF EACH EMAIL NOTIFICATION SENT.
 *  2) DEVELOP CONSOLE APPLICATION THAT DOES THE FOLLOWING STEPS:
 *      2.1) LOOKS FOR ALL RECEIPTS WITH STATUS AT YARD
 *      2.2) GETS ALL THE LOGISTICS RELATED TO EACH RECEIPT
 *      2.3) GETS THE ORDERS FOR EACH LOGISTIC
 *      2.4) EXPORTS A PDF FILE FOR EACH ORDER
 *      2.5) GETS THE CONTACTS FOR THE LOGISTIC FROM THE CUSTOM DATABASE (EELCOGAV)
 *      2.6) SENDS AN EMAIL FOR EACH LOGISTIC AND ATTACHES ALL THE ORDERS
 *  
 * 
 *  VERSION HISTORY:
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  1.0   IMPLEMENTATION DATE: MAY 25, 2017
 *      - USER NEEDS TO ENTER THE LOGISTIC INITIALS FOR EACH RECEIPT UNDER THE FIELD "MARKS" (IMEXNET)
 *      - APPLICATION IS EXECUTED EVERY 10 MINUTES IN SERVER01 (POINTING TO PRODUCTION SERVER: SQLDB)
 * 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
namespace LogisticsNotification
{
    class Program
    {
        static SqlConnection conexIMEX = new SqlConnection(GetConnectionString("IMEX"));
        static SqlDataReader dataIMEX;
        static SqlCommand queryIMEX;
        static ArrayList receiptsArray = new ArrayList();
        static string currentReceipt;
        static void Main(string[] args)
        {
            try
            {
                ProcessReceipts();
            }
            catch(Exception error)
            {
                if (!dataIMEX.IsClosed) { dataIMEX.Close(); }
                if (conexIMEX.State != ConnectionState.Closed) { conexIMEX.Close(); }
                NotifyError("Receipt: " + currentReceipt + ". Error: " + error.Message);
            }
        }

        private static void ProcessReceipts()
        {
            /* IDENTIFY THE RECEIPTS THAT ARE ALREADY AT YARD AND NEED TO BE NOTIFIED. AVOIDS RECEIPT FROM PERLICK, CUSTOMER ID 3901 */            
            queryIMEX = new SqlCommand("select R.OID as 'ReceiptId',R.WhseReceiptNo from WhseReceipt R join WhseReceiptLog L on(R.OID=L.WReceiptRel) where R.WhseReceiptNo not in(select receipt_number from EELCOGAV.dbo.MailNotification) and L.NewStatus=25 and L.CreatedAt>='06/10/2017 00:00:00.000' and R.ReceivedDate>='06/3/2017 00:00:00.000' UNION select R.OID, R.WhseReceiptNo from WhseReceipt R join DocBase D on(R.OID=D.OID) join Traffic T on(T.OID=D.Traffic) where R.RStatus >= 25 and R.OID not in(select WReceiptRel from WhseReceiptLog where NewStatus = 25) and R.ReceivedDate >= '06/08/2017' and R.WhseReceiptNo not in(select receipt_number from EELCOGAV.dbo.MailNotification) and T.Customer not in (3901)", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@StartNumber", ConfigurationManager.AppSettings["StartNumber"]);
            conexIMEX.Open();
            dataIMEX = queryIMEX.ExecuteReader();
            receiptsArray.Clear();
            while (dataIMEX.Read())
            {
                /* SAVE THE IDENTIFIED RECEIPT INTO AN ARRAY */
                string receiptId = dataIMEX["ReceiptId"].ToString().Trim();
                string receiptNo = dataIMEX["WhseReceiptNo"].ToString().Trim();
                //receiptsArray.Add(receiptId + "," + receiptNo);
            }
            dataIMEX.Close();
            conexIMEX.Close();
            
            //For testing
            receiptsArray.Add("435179,0000012993");
            receiptsArray.Add("435234,0000012999");
            receiptsArray.Add("435551,0000013016");
            receiptsArray.Add("435638,0000013023");
            receiptsArray.Add("435644,0000013024");
            receiptsArray.Add("435678,0000013026");
            receiptsArray.Add("435832,0000013027");
            receiptsArray.Add("435979,0000013036");
            receiptsArray.Add("436049,0000013040");
            receiptsArray.Add("436084,0000013043");
            receiptsArray.Add("436380,0000013054");
            receiptsArray.Add("436503,0000013058");
            receiptsArray.Add("436560,0000013061");
            receiptsArray.Add("436605,0000013066");

            /* LOOP THROUGH THE RECEIPTS ARRAY AND PROCESS EACH */
            foreach (string receipt in receiptsArray)
            {
                currentReceipt = receipt;
                string[] line = receipt.Split(',');
                string receiptId = line[0];
                string receiptNo = line[1];

                /* MAKE SURE THE RECEIPT HAS ALL LINES WITH A LOGISTIC BEFORE PROCESSING */                
                //if (GetLinesWithMissingData(receiptId) > 0)
                //{
                //    MailMessage mail = new MailMessage();                    
                //    mail.To.Add(ConfigurationManager.AppSettings["CC"]);
                //    mail.From = new MailAddress(ConfigurationManager.AppSettings["SenderMail"]);
                //    mail.Body = "PLEASE ADD ORDERS NUMBER AND LOGISTICS FOR ALL LINES IN RECEIPT " + receiptNo;
                //    mail.Subject = "MISSING DATA FOR RECEIPT " + receiptNo;
                //    mail.IsBodyHtml = true;
                //    SendMail(mail);
                //    continue;
                //}

                /* IDENTIFY HOW MANY LOGISTICS EXISTS IN THE CURRENT RECEIPT LINES */
                ArrayList logisticArray = GetLogisticsFromReceipt(receiptId);

                /* FOR EACH LOGISTIC IN THE ARRAY, GET THE ORDERS AND CONTACTS FROM THAT LOGISTIC AND SEND THE NOTIFICATION EMAIL */
                foreach (string logistic in logisticArray)
                {
                    MailMessage mail = new MailMessage();
                    string orders = GetOrdersFromLogistic(receiptId, logistic);
                    string[] orderList = orders.Split(',');
                    string equipment = GetEquipmentFromBL(receiptId);
                    string customer = GetCustomerFromBL(receiptId);
                    string contacts = "";
                    /* ENABLE THIS LINE BELOW AFTER TESTING HAS BEEN COMPLETE */
                    if(customer.Trim() != "")
                    {
                        contacts = GetContactsFromLogistic(logistic, customer);                    
                    }
                    ///contacts = "victor.basto@outlook.com"; // ConfigurationManager.AppSettings["CC"];
                    if (contacts.Trim() == "")
                    {
                        contacts = "shipping@uscustombroker.com";
                    }
                    
                    foreach (string order in orderList)
                    {
                        GenerarPDF(ref mail, receiptId, order, logistic);    
                    }
                    if (logistic == "CPL" || logistic == "CPU")
                    {
                        CreateCPLMail(ref mail, equipment, contacts);
                    }
                    else
                    {
                        CreateMail(ref mail, equipment, contacts);
                    }                    
                    SendMail(mail);
                    //CreateTransaction(receiptNo, logistic, contacts, orders);
                }
            }
        }

        private static ArrayList GetLogisticsFromReceipt(string receiptID)
        {
            ArrayList aux = new ArrayList();
            queryIMEX = new SqlCommand("select distinct Marks from WhseReceiptGood where WhseReceipt=@receiptID and CustomerOrderNo is not null and isnull(Marks,'')<>'' order by Marks", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            conexIMEX.Open();
            dataIMEX = queryIMEX.ExecuteReader();
            while (dataIMEX.Read())
            {
                aux.Add(dataIMEX["Marks"].ToString());
            }
            dataIMEX.Close();
            conexIMEX.Close();
            return aux;
        }

        private static int GetLinesWithMissingData(string receiptID)
        {
            int res;
            queryIMEX = new SqlCommand("select count(*) from WhseReceiptGood where WhseReceipt=@receiptID and (CustomerOrderNo is null or Marks is null)", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            conexIMEX.Open();
            res = (int) queryIMEX.ExecuteScalar();
            conexIMEX.Close();
            return res;
        }

        private static string GetCustomerFromBL(string receiptID)
        {
            string res;
            queryIMEX = new SqlCommand("select top 1 Customer from Traffic where TrafficNo in(select BLNo from WhseReceiptBL where Receipt=@receiptID)", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            conexIMEX.Open();
            if (queryIMEX.ExecuteScalar() != null)
            {
                res = queryIMEX.ExecuteScalar().ToString();
            }
            else
            {
                res = "";
            }
            conexIMEX.Close();
            return res;
        }

        private static string GetEquipmentFromBL(string receiptID)
        {
            string res;
            queryIMEX = new SqlCommand("select isnull(TrailerNo,'') + ' ' + isnull(EquipmentProvider,'') as 'Equipment' from WhseReceiptBL where Receipt=@receiptID", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            conexIMEX.Open();
            res = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return res;
        }

        private static string GetOrdersFromLogistic(string receiptID, string logistic)
        {
            string res;
            queryIMEX = new SqlCommand("declare @orders varchar(max); set @orders = ''; select @orders = @orders + T.orderNumber + ',' from(select distinct CustomerOrderNo as 'orderNumber' from WhseReceiptGood where WhseReceipt = @receiptID and CustomerOrderNo is not null and Marks = @logistic) as T; if(rtrim(@orders)<>'') begin select substring(@orders,1,len(@orders)-1) end else begin select @orders end", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            queryIMEX.Parameters.AddWithValue("@logistic", logistic);
            conexIMEX.Open();
            res = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return res;
        }

        private static string GetProductsFromOrder(string receiptID, string order)
        {
            string res;
            queryIMEX = new SqlCommand("declare @products varchar(max); set @products = ''; select @products = @products + rtrim(Product) + ',' from WhseReceiptGood where WhseReceipt = @receiptID and CustomerOrderNo = @order; if(rtrim(@products)<>'') begin select substring(@products,1,len(@products)-1) end else begin select @products end", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            queryIMEX.Parameters.AddWithValue("@order", order);
            conexIMEX.Open();
            res = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return res;
        }

        private static string GetSerialsFromOrder(string receiptID, string order, string product)
        {
            string res;
            queryIMEX = new SqlCommand("declare @serials varchar(max); set @serials = ''; select @serials = @serials + rtrim(Reference) + ',' from WhseReceiptGoodReference where WhseReceiptGood in(select OID from WhseReceiptGood where WhseReceipt = @receiptID and CustomerOrderNo = @order and Product=@product); if(rtrim(@serials)<>'') begin select substring(@serials,1,len(@serials)-1) end else begin select @serials end", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            queryIMEX.Parameters.AddWithValue("@order", order);
            queryIMEX.Parameters.AddWithValue("@product", product);
            conexIMEX.Open();
            res = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return res;
        }

        private static void CreateTransaction(string receiptNo, string logistic, string contacts, string orders)
        {
            queryIMEX = new SqlCommand("insert into EELCOGAV.dbo.MailNotification(receipt_number,notification_date,logistic,contacts,orders) values(@receiptNo,getdate(),@logistic,@contacts,@orders)", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptNo", receiptNo);
            queryIMEX.Parameters.AddWithValue("@logistic", logistic);
            queryIMEX.Parameters.AddWithValue("@contacts", contacts);
            queryIMEX.Parameters.AddWithValue("@orders", orders);
            conexIMEX.Open();
            queryIMEX.ExecuteNonQuery();
            conexIMEX.Close();
        }

        private static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        private static string GetContactsFromLogistic(string logistic, string customer)
        {
            string contacts = "";
            queryIMEX = new SqlCommand("declare @contacts varchar(max); set @contacts = ''; select @contacts = @contacts + Email + ',' from EELCOGAV.dbo.CAT_Logistics L join EELCOGAV.dbo.CAT_LogisticsContact C on(C.LogisticID = L.LogisticID) join EELCOGAV.dbo.txCustomerLogistic U on(U.LogisticID=L.LogisticID) where L.Code = @logistic and L.Active = 1 and U.CustomerID=@customer; if(rtrim(@contacts)<>'') begin select substring(@contacts,1,len(@contacts)-1) end else begin select @contacts end", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@logistic", logistic);
            queryIMEX.Parameters.AddWithValue("@customer", customer);
            conexIMEX.Open();
            contacts = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return contacts;
        }

        private static void CreateMail(ref MailMessage mail, string equipment, string contacts)
        {
            mail.To.Add(contacts);
            mail.CC.Add(ConfigurationManager.AppSettings["CC"]);
            mail.From = new MailAddress(ConfigurationManager.AppSettings["SenderMail"]);
            mail.Body = ConfigurationManager.AppSettings["MailNotificationBody"];
            mail.Subject = "TRUCK # " + equipment;
            mail.IsBodyHtml = true;
        }

        private static void CreateCPLMail(ref MailMessage mail, string equipment, string contacts)
        {
            mail.To.Add(contacts);
            mail.CC.Add(ConfigurationManager.AppSettings["CC"]);
            mail.From = new MailAddress(ConfigurationManager.AppSettings["SenderMail"]);
            mail.Body = ConfigurationManager.AppSettings["CPLNotificationBody"];
            mail.Subject = "TRUCK # " + equipment;
            mail.IsBodyHtml = true;
        }

        private static void SendMail(MailMessage mail)
        {
            SmtpClient smtp = new SmtpClient()
            {
                Host = ConfigurationManager.AppSettings["SMTPHost"],                
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], ConfigurationManager.AppSettings["SMTPPassword"]),
                Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]),
                EnableSsl = true
            };
            smtp.Send(mail);
        }
        
        private static void NotifyError(string message)
        {
            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(ConfigurationManager.AppSettings["SenderMail"]),
                Subject = "EELCO, Application error - Logistics Notification",
                IsBodyHtml = true,
                Body = message
            };
            mail.To.Add(ConfigurationManager.AppSettings["IT"]);
            SendMail(mail);
        }

        private static void GenerarPDF(ref MailMessage msg, string receiptID, string order, string logistic)
        {
            Font Arial13 = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
            Font Arial10 = FontFactory.GetFont("Helvetica", 10, Font.NORMAL, BaseColor.BLACK);
            Font Arial10Underline = FontFactory.GetFont("Helvetica", 10, Font.UNDERLINE, BaseColor.BLACK);
            Font Arial9 = FontFactory.GetFont("Helvetica", 9, BaseColor.BLACK);
            Font Arial8 = FontFactory.GetFont("Helvetica", 9, BaseColor.BLACK);
            Font Arial8Bold = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.BLACK);
            string VarCliNom = "", VarCliDireccion = "", VarCliCiudad = "", VarReceiptNo = "", VarReceiptDate = "", VarEquipment = "", VarOrder = "", VarConName = "", VarConDireccion = "", VarConCiudad = "", VarQtyUOM = "", VarNetLBS = "", VarLogistic = "", VarProduct = "", VarProductDesc = "", VarInstructions = "", VarLogisticFullName = "";
            ArrayList productArray = new ArrayList();
            try
            {
                //Get the values from the database and put them into the report variables
                queryIMEX = new SqlCommand("select rtrim(C.name) as 'CustomerName' " + 
                                         ", rtrim(CA.Street) + ', ' + rtrim(CA.ZipCode) as 'CustomerAddress' " +
                                         ", rtrim(CA.City) + ', ' + rtrim(S.Name) + ', ' + rtrim(CO.Name) as 'CustomerCity' " +
                                         ", R.WhseReceiptNo as 'ReceiptNo' " +
                                         ", R.ReceivedDate as 'ReceivedDate' " +
                                         ", isnull(B.DeliveredBy, B.EquipmentProvider) + '  ' + isnull(B.TrailerNo, '') as 'Equipment' " +
                                         ", RG.CustomerOrderNo as 'OrderNo' " +
                                         ", E.name as 'ConsigneeName' " +
                                         ", rtrim(EA.Street) + ', ' + rtrim(EA.ZipCode) as 'ConsigneeAddress' " +
                                         ", rtrim(EA.City) + ',' + isnull(SC.code,'') + ',' + isnull(CC.Name,'')  as 'ConsigneeCity' " +
                                         ", SUM(RG.CommercialQty) as 'Quantity' " +
                                         ", SUM(RG.NetWeight) as 'NetWeight' " +
                                         ", P.ProductNo + ' ' + isnull(RG.ProdDescription,'') as 'ProductDesc' " +
                                         ", P.OID as 'productID' " +
                                         ", B.Instructions as 'Instructions' " +
                                         ", (select Name from EELCOGAV.dbo.CAT_Logistics where Code=@logistic) as '3PL' " +
                                         "from WhseReceipt R " +
                                         "join WhseReceiptBL B on (B.Receipt = R.OID) " +
                                         "join Traffic T on (T.TrafficNo = B.BLNo) " +
                                         "join Customer C on (C.OID = T.Customer) " +
                                         "cross apply ( select top 1 * from CustAddress where Customer = C.OID order by OID desc) CA " +
                                         "join WhseReceiptGood RG on(RG.WhseReceipt = R.OID) " +
                                         "join WhseConsignee W on(W.OID = RG.RelWhseConsignee) " +
                                         "join EntityBase E on(E.OID = W.OID) " +
                                         "join EntityAddress EA on(EA.OID = E.Address) " +
                                         "join State S on(S.OID = CA.State) " +
                                         "join Country CO on(CO.OID = CA.Country) " +
                                         "join Product P on(RG.Product=P.OID) " +
                                         "left join State SC on(SC.OID=EA.State) " +
                                         "left join Country CC on(CC.OID = EA.Country) " + 
                                         "where R.OID = @ReceiptID " + 
                                         "and RG.CustomerOrderNo = @OrderNo " +
                                         "group by rtrim(C.name) " +
                                         "       , rtrim(CA.Street) + ', ' + rtrim(CA.ZipCode) " +
                                         "       , rtrim(CA.City) + ', ' + rtrim(S.Name) + ', ' + rtrim(CO.Name) " +
                                         "       , R.WhseReceiptNo " +
                                         "       , R.ReceivedDate " +
                                         "       , isnull(B.TrailerNo, '') + ' - ' + isnull(B.EquipmentProvider, '') " +
                                         "       , RG.CustomerOrderNo " +
                                         "       , E.name " +
                                         "       , rtrim(EA.Street) + ', ' + rtrim(EA.ZipCode) " +
                                         "       , rtrim(EA.City) " +
                                         "       , P.ProductNo " +
                                         "       , RG.ProdDescription " +
                                         "       , P.OID " +
                                         "       , B.DeliveredBy " +
                                         "       , B.Instructions " +
                                         "       , B.EquipmentProvider " +
                                         "       , B.TrailerNo " +
                                         "       , SC.Code " +
                                         "       , CC.Name", conexIMEX);
                queryIMEX.Parameters.AddWithValue("@ReceiptID", receiptID);
                queryIMEX.Parameters.AddWithValue("@OrderNo", order);
                queryIMEX.Parameters.AddWithValue("@logistic", logistic);
                conexIMEX.Open();
                dataIMEX = queryIMEX.ExecuteReader();
                while (dataIMEX.Read())
                {
                    VarCliNom = dataIMEX["CustomerName"].ToString();
                    VarCliDireccion = dataIMEX["CustomerAddress"].ToString();
                    VarCliCiudad = dataIMEX["CustomerCity"].ToString();
                    VarReceiptNo = dataIMEX["ReceiptNo"].ToString();
                    VarReceiptDate = dataIMEX["ReceivedDate"].ToString();
                    VarEquipment = dataIMEX["Equipment"].ToString();
                    VarOrder = dataIMEX["OrderNo"].ToString();
                    VarConName = dataIMEX["ConsigneeName"].ToString();
                    VarConDireccion = dataIMEX["ConsigneeAddress"].ToString();
                    VarConCiudad = dataIMEX["ConsigneeCity"].ToString();
                    VarQtyUOM = dataIMEX["Quantity"].ToString();
                    //VarDescription = dataIMEX["Description"].ToString();
                    VarNetLBS = dataIMEX["NetWeight"].ToString();
                    VarLogistic = logistic;
                    VarProduct = dataIMEX["productID"].ToString();
                    VarProductDesc = dataIMEX["ProductDesc"].ToString();
                    VarLogisticFullName = dataIMEX["3PL"].ToString();

                    productArray.Add(VarProduct + "|" + VarQtyUOM + "|" + VarProductDesc + "|" + VarNetLBS + "|");
                }
                dataIMEX.Close();
                conexIMEX.Close();


                Document doc = new Document(iTextSharp.text.PageSize.LETTER, 14.0F, 14.0F, 14.0F, 14.0F);
                MemoryStream ms = new MemoryStream();
                PdfWriter pw = PdfWriter.GetInstance(doc, ms);                
                doc.Open();                
                PdfContentByte cb = pw.DirectContent;
                PdfContentByte pcb = pw.DirectContent;
                PdfPTable tblBoleta = new PdfPTable(4);
                tblBoleta.WidthPercentage = 100;

                AddCell(tblBoleta, "EELCO Supply Chain Solutions", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                    Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 1, 0);
                AddCell(tblBoleta, "PICK UP NOTIFICATION", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "8411 WHITEPOINT RD, LAREDO TX 78045", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_CENTER,
                   Element.ALIGN_CENTER, 0, 1, 0, 0, 8, 2, 0);

                AddCell(tblBoleta, "CUSTOMER INFORMATION", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                    Element.ALIGN_CENTER, 0, 1, 0, 0, 5, 2, 1);

                AddCell(tblBoleta, VarCliNom, 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 5, 0);
                AddCell(tblBoleta, VarCliDireccion, 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarCliCiudad, 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);

                doc.Add(tblBoleta);

                tblBoleta = new PdfPTable(6);
                tblBoleta.WidthPercentage = 100;

                AddCell(tblBoleta, "WAREHOUSE RECEIPT INFORMATION", 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                    Element.ALIGN_CENTER, 1, 1, 0, 0, 5, 2, 1);

                AddCell(tblBoleta, "Receipt No:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 5, 0);
                AddCell(tblBoleta, VarReceiptNo, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 5, 0);
                AddCell(tblBoleta, "Receipt Date:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarReceiptDate, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Equipment:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarEquipment, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "3PL:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);
                AddCell(tblBoleta, VarLogistic + " - " + VarLogisticFullName, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);
                //AddCell(tblBoleta, "Instructions:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);
                //AddCell(tblBoleta, VarInstructions, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);

                doc.Add(tblBoleta);

                tblBoleta = new PdfPTable(8);
                tblBoleta.WidthPercentage = 100;

                AddCell(tblBoleta, "ORDER INFORMATION", 1, 8, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                    Element.ALIGN_CENTER, 1, 1, 0, 0, 5, 5, 1);

                AddCell(tblBoleta, "Order", 1, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 1, 1, 0, 3, 2, 1);
                AddCell(tblBoleta, "Consignee", 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 1, 0, 0, 3, 2, 1);
                AddCell(tblBoleta, VarOrder, 3, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 1, 1, 0, 5, 2, 0);
                AddCell(tblBoleta, VarConName, 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarConDireccion, 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarConCiudad, 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                   Element.ALIGN_CENTER, 0, 1, 0, 0, 5, 2, 0);
                                
                foreach (string product in productArray)
                {
                    string[] aux = product.Split('|');
                    string auxProductID = aux[0].Trim();
                    string auxQty = aux[1].Trim();
                    string auxDescription = aux[2].Trim();
                    string auxWeight = aux[3].Trim();

                    AddCell(tblBoleta, "Qty / UOM", 1, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 1, 0, 3, 2, 1);
                    AddCell(tblBoleta, "Description", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 1, 0, 3, 2, 1);
                    AddCell(tblBoleta, "Net(Lbs):", 1, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 0, 0, 3, 2, 1);
                    AddCell(tblBoleta, auxQty, 1, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                       Element.ALIGN_CENTER, 0, 1, 1, 0, 5, 2, 0);
                    AddCell(tblBoleta, auxDescription, 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 1, 0, 5, 2, 0);
                    AddCell(tblBoleta, auxWeight, 1, 2, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 0, 0, 5, 2, 0);

                    AddCell(tblBoleta, "Reference(s)", 1, 8, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 0, 0, 3, 2, 1);
                    AddCell(tblBoleta, GetSerialsFromOrder(receiptID, order, auxProductID), 1, 8, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT,
                    Element.ALIGN_CENTER, 0, 1, 0, 0, 5, 2, 0);
                }

                doc.Add(tblBoleta);
                pw.CloseStream = false;
                doc.Close();
                ms.Position = 0;

                //Attach the order report to the email
                msg.Attachments.Add(new Attachment(ms, order + ".pdf"));
            }
            catch (Exception ex)
            {

            }
        }

        protected static void CreateDContent(PdfContentByte cb, int size, int xpos, int ypos, string texto)
        {
            BaseFont f_cn;
            cb.BeginText();
            f_cn = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetFontAndSize(f_cn, size);
            cb.SetTextMatrix(xpos, ypos);
            cb.ShowText(texto);
            cb.EndText();
        }

        protected static void AddCell(PdfPTable tbl, string txt, int rowspan, int colspan, Font font, int HorAlign, int VerAlign,
            float BorderT, float BorderB, float BorderR, float BorderL, float PaddingBottom, float PaddingTop, int addcolor)
        {
            PdfPCell c = new PdfPCell(new Phrase(txt, font))
            {
                Rowspan = rowspan,
                Colspan = colspan,
                HorizontalAlignment = HorAlign,
                VerticalAlignment = VerAlign,
                BorderWidthBottom = BorderB,
                BorderWidthTop = BorderT,
                BorderWidthLeft = BorderL,
                BorderWidthRight = BorderR,
                PaddingBottom = PaddingBottom,
                PaddingTop = PaddingTop
            };
            if (addcolor == 1)
            {
                c.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            }
            tbl.AddCell(c);
        }
    }
}