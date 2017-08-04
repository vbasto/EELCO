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
 *  DESCRIPTION:
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  SEND AN AUTOMATIC NOTIFICATION EMAIL TO PERLICK WHEN A LOAD HAS ARRIVED TO EELCO. ATTACH A CUSTOM REPORT WITH INFORMATION OF RECEIVED ITEMS.
 *    
 * 
 *  VERSION HISTORY:
 *  --------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 *  1.0   IMPLEMENTATION DATE: JUNE 9, 2017
  *      - APPLICATION IS EXECUTED EVERY 10 MINUTES IN SERVER01 (POINTING TO PRODUCTION SERVER: SQLDB)
 * 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
namespace LogisticsNotificationPerlick
{
    class Program
    {
        static SqlConnection conexIMEX = new SqlConnection(GetConnectionString("IMEX"));
        static SqlDataReader dataIMEX;
        static SqlCommand queryIMEX;
        static ArrayList receiptsArray = new ArrayList();

        static void Main(string[] args)
        {
            try
            {
                ProcessReceipts();
            }
            catch (Exception error)
            {
                if (!dataIMEX.IsClosed) { dataIMEX.Close(); }
                if (conexIMEX.State != ConnectionState.Closed) { conexIMEX.Close(); }
                NotifyError(error.Message);
            }
        }

        private static void ProcessReceipts()
        {
            /* IDENTIFY THE RECEIPTS THAT MUST BE NOTIFIED */
            queryIMEX = new SqlCommand("select R.OID as 'ReceiptId', R.WhseReceiptNo from WhseReceipt R join DocBase D on (D.OID = R.OID) join Traffic T on (T.OID = D.Traffic) join Customer C on (T.Customer = C.OID) where T.Customer = @CustomerId and R.WhseReceiptNo not in(select receipt_number from EELCOGAV.dbo.PerlickNotification) and R.RStatus=40", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@CustomerId", 3901); //ID de Perlick
            conexIMEX.Open();
            dataIMEX = queryIMEX.ExecuteReader();
            receiptsArray.Clear();
            while (dataIMEX.Read())
            {
                /* SAVE RECEIPTS INTO AN ARRAY */
                string receiptId = dataIMEX["ReceiptId"].ToString().Trim();
                string receiptNo = dataIMEX["WhseReceiptNo"].ToString().Trim();
                receiptsArray.Add(receiptId + "," + receiptNo);
            }
            dataIMEX.Close();
            conexIMEX.Close();

            /* LOOP THROUGH THE RECEIPTS ARRAY AND PROCESS EACH */
            foreach (string receipt in receiptsArray)
            {
                string[] line = receipt.Split(',');
                string receiptId = line[0];
                string receiptNo = line[1];
                
                MailMessage mail = new MailMessage();
                string[] orders = GetOrdersFromReceipt(receiptId).Split(',');
                foreach (string order in orders)
                {
                    GenerarPDF(ref mail, receiptId, order, "");
                }

                string EntryDate = "", Customer = "", Supplier = "", Bultos = "", Carrier = "", Trailer = "";
                queryIMEX = new SqlCommand("select top 1 R.ReceivedDate as 'EntryDate', C.Name as 'Customer', E.Name as 'Supplier', B.Packages as 'Bultos', B.EquipmentProvider as 'Carrier', TrailerNo as 'Trailer' from WhseReceipt R join DocBase D on (R.OID = D.OID) join Traffic T on (T.OID = D.Traffic) join WhseReceiptBL B on (B.Receipt = R.OID) join Customer C on (C.OID = T.Customer) join EntityBase E on (E.OID = R.ShipBy) where R.WhseReceiptNo = @WhseReceiptNo", conexIMEX);
                queryIMEX.Parameters.AddWithValue("@WhseReceiptNo", receiptNo);
                conexIMEX.Open();
                dataIMEX = queryIMEX.ExecuteReader();
                while (dataIMEX.Read())
                {
                    EntryDate = dataIMEX["EntryDate"].ToString().Trim();
                    Customer = dataIMEX["Customer"].ToString().Trim();
                    Supplier = dataIMEX["Supplier"].ToString().Trim();
                    Bultos = dataIMEX["Bultos"].ToString().Trim();
                    Carrier = dataIMEX["Carrier"].ToString().Trim();
                    Trailer = dataIMEX["Trailer"].ToString().Trim();
                }
                dataIMEX.Close();
                conexIMEX.Close();

                string contacts = ConfigurationManager.AppSettings["Contacts"];

                CreateMail(ref mail, contacts, receiptNo, EntryDate, Customer, Supplier, Bultos, Carrier, Trailer);
                SendMail(mail);
                CreateTransaction(receiptNo, contacts);
            }
        }
        
        private static string GetOrdersFromReceipt(string receiptID)
        {
            string res;
            //queryIMEX = new SqlCommand("declare @orders varchar(max); set @orders = ''; select @orders = @orders + T.orderNumber + ',' from(select distinct CustomerOrderNo as 'orderNumber' from WhseReceiptGood where WhseReceipt = @receiptID and CustomerOrderNo is not null) as T; select substring(@orders,1,len(@orders)-1)", conexIMEX);
            queryIMEX = new SqlCommand("select isnull(CustomerReference,'') as 'CustomerReference' from WhseReceipt where OID=@receiptID", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptID", receiptID);
            conexIMEX.Open();
            res = queryIMEX.ExecuteScalar().ToString();
            conexIMEX.Close();
            return res;
        }

        private static void CreateTransaction(string receiptNo, string contacts)
        {
            queryIMEX = new SqlCommand("insert into EELCOGAV.dbo.PerlickNotification(receipt_number,notification_date,contacts) values(@receiptNo,getdate(),@contacts)", conexIMEX);
            queryIMEX.Parameters.AddWithValue("@receiptNo", receiptNo);
            queryIMEX.Parameters.AddWithValue("@contacts", contacts);
            conexIMEX.Open();
            queryIMEX.ExecuteNonQuery();
            conexIMEX.Close();
        }

        private static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        private static void CreateMail(ref MailMessage mail,string contacts, string WhseReceiptNo, string EntryDate, string Customer, string Supplier, string Bultos, string Carrier, string Trailer)
        {
            mail.To.Add(contacts);
            mail.CC.Add(ConfigurationManager.AppSettings["CC"]);
            mail.From = new MailAddress(ConfigurationManager.AppSettings["SenderMail"]);
            mail.Body = "EELCO Supply Chain Solutions<br/>EDUARDO E LOZANO & CO INC<br/><br/><br/>ARRIVAL REFERENCE: @WhseReceiptNo <br/>ENTRY DATE: @EntryDate <br/> CUSTOMER: @Customer <br/>SUPPLIER: @Supplier <br/> BULTOS: @Bultos <br/> CARRIER: @Carrier <br/>TRAILER / TRACKING NO: @Trailer";
            mail.Body = mail.Body.Replace("@WhseReceiptNo", WhseReceiptNo).Replace("@EntryDate", EntryDate).Replace("@Customer", Customer).Replace("@Supplier", Supplier).Replace("@Bultos", Bultos).Replace("@Carrier", Carrier).Replace("@Trailer", Trailer);
            mail.Subject = "EELCO ARRIVAL - REFERENCE " + WhseReceiptNo; 
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
                Subject = "EELCO, Application error - Perlick Notification",
                IsBodyHtml = true,
                Body = message
            };
            mail.To.Add(ConfigurationManager.AppSettings["IT"]);
            SendMail(mail);
        }

        private static void GenerarPDF(ref MailMessage msg, string receiptID, string order, string serials)
        {
            Font Arial13 = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
            Font Arial10 = FontFactory.GetFont("Helvetica", 10, Font.NORMAL, BaseColor.BLACK);
            Font Arial10Underline = FontFactory.GetFont("Helvetica", 10, Font.UNDERLINE, BaseColor.BLACK);
            Font Arial9 = FontFactory.GetFont("Helvetica", 9, BaseColor.BLACK);
            Font Arial8 = FontFactory.GetFont("Helvetica", 9, BaseColor.BLACK);
            Font Arial8Bold = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.BLACK);
            string VarCliNom = "", VarCliDireccion = "", VarCliCiudad = "", VarReceiptNo = "", VarReceiptDate = "", VarEquipment = "", VarOrder = "", VarConName = "", VarConDireccion = "", VarConCiudad = "", VarQtyUOM = "", VarDescription = "", VarNetLBS = "", VarReferences = "", VarLogistic = "";
            string VarCarrier = "", VarTrailer = "", VarPackages = "", VarGrossWeight = "", VarUOM = "", VarSupplier = "";
            try
            {
                //Get the values from the database and put them into the report variables
                queryIMEX = new SqlCommand("select R.WhseReceiptNo as 'ReceiptNo', R.ReceivedDate as 'ReceivedDate', isnull(R.CustomerReference,'') as 'OrderNo', B.EquipmentProvider as 'Carrier', B.TrailerNo as 'Trailer', B.Packages as 'Packages', B.GrossWeight as 'GrossWeight', 'PZA' as 'UOM', (select name from EntityBase where oid=R.ShipBy) as 'Supplier' " +
                                           "from WhseReceipt R left join WhseReceiptBL B on (B.Receipt = R.OID) " +
                                           "where R.OID = @ReceiptID and isnull(R.CustomerReference, '') = @OrderNo " +
                                           "group by R.WhseReceiptNo, R.ReceivedDate, R.CustomerReference, B.EquipmentProvider, B.TrailerNo, B.Packages, B.GrossWeight, R.ShipBy", conexIMEX);
                queryIMEX.Parameters.AddWithValue("@ReceiptID", receiptID);
                queryIMEX.Parameters.AddWithValue("@OrderNo", order);
                conexIMEX.Open();
                dataIMEX = queryIMEX.ExecuteReader();
                while (dataIMEX.Read())
                {
                    VarReceiptNo = dataIMEX["ReceiptNo"].ToString();
                    VarReceiptDate = dataIMEX["ReceivedDate"].ToString();
                    //VarEquipment = dataIMEX["Equipment"].ToString();
                    VarOrder = dataIMEX["OrderNo"].ToString();
                    //VarConName = dataIMEX["ConsigneeName"].ToString();
                    //VarConDireccion = dataIMEX["ConsigneeAddress"].ToString();
                    //VarConCiudad = dataIMEX["ConsigneeCity"].ToString();
                    //VarQtyUOM = dataIMEX["Quantity"].ToString();
                    //VarDescription = dataIMEX["Description"].ToString();
                    //VarNetLBS = dataIMEX["NetWeight"].ToString();
                    //VarReferences = serials;

                    VarCarrier= dataIMEX["Carrier"].ToString();
                    VarTrailer = dataIMEX["Trailer"].ToString();
                    VarPackages = dataIMEX["Packages"].ToString();
                    VarGrossWeight = dataIMEX["GrossWeight"].ToString();
                    VarUOM = dataIMEX["UOM"].ToString();
                    VarSupplier = dataIMEX["Supplier"].ToString();
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
                AddCell(tblBoleta, "ARRIVAL NOTIFICATION", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                   Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "8411 WHITEPOINT RD, LAREDO TX 78045", 1, 4, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_CENTER,
                   Element.ALIGN_CENTER, 0, 1, 0, 0, 8, 2, 0);

                doc.Add(tblBoleta);

                tblBoleta = new PdfPTable(6);
                tblBoleta.WidthPercentage = 100;

                AddCell(tblBoleta, "WAREHOUSE RECEIPT INFORMATION", 1, 6, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_CENTER,
                    Element.ALIGN_CENTER, 1, 1, 0, 0, 5, 2, 1);

                AddCell(tblBoleta, "Receipt No:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 5, 0);
                AddCell(tblBoleta, VarReceiptNo, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 5, 0);
                AddCell(tblBoleta, "Receipt Date:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarReceiptDate, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Carrier:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarCarrier, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Trailer/Tracking No:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarTrailer, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Packages:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarPackages, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Gross Weight:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarGrossWeight, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "UOM:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarUOM, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Supplier:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, VarSupplier, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 2, 2, 0);
                AddCell(tblBoleta, "Order Number:", 1, 1, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.BOLD), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);
                AddCell(tblBoleta, VarOrder, 1, 5, FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharp.text.Font.NORMAL), Element.ALIGN_LEFT, Element.ALIGN_CENTER, 0, 0, 0, 0, 8, 2, 0);

                doc.Add(tblBoleta);

                tblBoleta = new PdfPTable(8);
                tblBoleta.WidthPercentage = 100;
                
                doc.Add(tblBoleta);
                pw.CloseStream = false;
                doc.Close();
                ms.Position = 0;

                //Attach the order report to the email
                msg.Attachments.Add(new Attachment(ms, VarReceiptNo + ".pdf"));
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