using System;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace PerlickManifestWeb
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                divConsignee.Visible = false;
                divPerlickManifest.Visible = false;
            }
        }

        protected void btProcess_Click(object sender, EventArgs e)
        {
            if (rbManifest.Checked)
            {
                if (!fuPerlickFile.HasFile)
                {
                    throw new ArgumentException("Necesita seleccionar un archivo.");
                }
                fuPerlickFile.SaveAs(MapPath("perlick.csv"));

                StreamReader srSource = new StreamReader(MapPath("perlick.csv"), Encoding.GetEncoding(1252));
                SqlConnection conexSQL = new SqlConnection(ConfigurationManager.AppSettings["DB"]);
                SqlCommand querySQL;
                SqlDataReader dataSQL;
                try
                {
                    //Variables para cambiar cuando la aplicacion este en produccion
                    string customerId = ConfigurationManager.AppSettings["DefaultCustomerId"];  //Cambiar el 3250 por el verdadero ID de Perlick
                    string createdBy = ConfigurationManager.AppSettings["DefaultEmployeeId"];   //Usuario de Gabriel. Confirmar si puede quedarse este ID
                    string DEFAULT_PRODUCT_ID = ConfigurationManager.AppSettings["DefaultProductId"]; //Cambiarlo por el ID del producto default que se va a agregar para los casos donde no exista el producto. Revisar con Gabriel.

                    string aux = "0000000000" + txtReceiptNo.Text.Trim();
                    string WhseReceiptNo = aux.Substring(aux.Trim().Length - 10);
                    string WhseReceiptId = "";
                    string currentLine = "";
                    string[] dataArray;
                    ArrayList items = new ArrayList();

                    //Variables para las columnas del archivo
                    string load, partNo, orderNo, qtyBalance, model, serial, invoice;
                    decimal netWeight;
                    decimal receiptWeight = 0;
                    int receiptPackages = 0;

                    /*Buscar el ID del receipt basandonos en el numero de orden que viene en el archivo*/
                    conexSQL.Open();
                    querySQL = new SqlCommand("select OID from WhseReceipt where WhseReceiptNo=@WhseReceiptNo", conexSQL);
                    querySQL.Parameters.AddWithValue("@WhseReceiptNo", WhseReceiptNo);
                    if (querySQL.ExecuteScalar() == null)
                    {
                        throw new ArgumentException("No existe ese numero de receipt");
                    }
                    else
                    {
                        WhseReceiptId = querySQL.ExecuteScalar().ToString();
                    }
                    conexSQL.Close();

                    /*Comenzar a leer archivo*/
                    srSource.ReadLine();        //Omitir encabezado
                    currentLine = srSource.ReadLine();
                    dataArray = currentLine.Split(',');
                    load = dataArray[0].Trim();

                    //Actualizar el CustomerReference de WhseReceipt con el LoadId del archivo
                    querySQL = new SqlCommand("update WhseReceipt set CustomerReference=@load where OID=@WhseReceiptId", conexSQL);
                    querySQL.Parameters.AddWithValue("@WhseReceiptId", WhseReceiptId);
                    querySQL.Parameters.AddWithValue("@load", load);
                    conexSQL.Open();
                    querySQL.ExecuteNonQuery();
                    conexSQL.Close();

                    //Regresar el archivo al inicio y omitir el encabezado
                    srSource.DiscardBufferedData();
                    srSource.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    srSource.ReadLine();

                    //Leer el archivo Source
                    while (srSource.Peek() != -1)
                    {
                        currentLine = srSource.ReadLine();
                        dataArray = currentLine.Split(',');
                        load = dataArray[0].Trim();
                        invoice = dataArray[1].Trim();
                        partNo = dataArray[3].Trim();
                        model = dataArray[5].Trim();
                        qtyBalance = dataArray[8].Trim();
                        netWeight = Convert.ToDecimal(dataArray[9].Trim());
                        serial = dataArray[10].Trim();
                        orderNo = dataArray[13].Trim();
                        receiptWeight += netWeight; //Ir guardando el peso de todas las lineas para despues guardarlo en WhseReceiptBL
                        if (serial.Trim() != "")
                        {
                            receiptPackages++;
                        }

                        bool existsMatch = false;
                        int index = -1;
                        foreach (string i in items)
                        {
                            index++;
                            string[] aux2 = i.Split(',');
                            if (orderNo.Trim() + "," + partNo.Trim() == aux2[0] + "," + aux2[1])
                            {
                                existsMatch = true;
                                break;
                            }
                        }
                        if (existsMatch)
                        {
                            string[] aux2 = items[index].ToString().Split(',');
                            items[index] = aux2[0].Trim() + "," + aux2[1].Trim() + "," + (Convert.ToInt16(aux2[2]) + 1).ToString() + "," + aux2[3] + "," + aux2[4] + (serial.Trim() != "" ? ";" + serial : "") + "," + (Convert.ToDecimal(aux2[5]) + Convert.ToDecimal(netWeight)).ToString();
                        }
                        else
                        {
                            items.Add(orderNo.Trim() + "," + partNo + ",1," + invoice + "," + serial + "," + netWeight.ToString());
                        }
                    }
                    srSource.Close();

                    //Ordenar por el numero de orden
                    items.Sort();

                    //Guardar en WhseReceiptBL los paquetes y el peso total
                    querySQL = new SqlCommand("update WhseReceiptBL set GrossWeight=@GrossWeight, Packages=@Packages where Receipt=@receipt", conexSQL);
                    querySQL.Parameters.AddWithValue("@GrossWeight", receiptWeight);
                    querySQL.Parameters.AddWithValue("@Packages", receiptPackages);
                    querySQL.Parameters.AddWithValue("@receipt", WhseReceiptId);
                    conexSQL.Open();
                    querySQL.ExecuteNonQuery();
                    conexSQL.Close();

                    foreach (string i in items)
                    {
                        string[] aux2 = i.Split(',');
                        orderNo = aux2[0];
                        partNo = aux2[1];
                        qtyBalance = aux2[2];
                        invoice = aux2[3];
                        string[] serialArray = aux2[4].Split(';');
                        netWeight = Convert.ToDecimal(aux2[5]);
                        string prodDescription = "";

                        string productId = DEFAULT_PRODUCT_ID;
                        //Obtener el ID del producto
                        querySQL = new SqlCommand("select top 1 OID, Description from Product where ProductNo=@ProductNo and Customer=@Customer", conexSQL);
                        querySQL.Parameters.AddWithValue("@ProductNo", partNo);
                        querySQL.Parameters.AddWithValue("@Customer", customerId);
                        conexSQL.Open();
                        dataSQL = querySQL.ExecuteReader();
                        while (dataSQL.Read())
                        {
                            productId = dataSQL[0].ToString().Trim();
                            prodDescription = dataSQL[1].ToString().Trim();
                        }
                        dataSQL.Close();
                        conexSQL.Close();

                        if (aux2[4].Trim() != "")
                        {
                            //Obtener la suma total por orden, para incluir el peso de los productos que no tienen una serie en el archivo
                            for (int x = 0; x < items.Count; x++)
                            {
                                int auxLength = orderNo.Trim().Length;
                                if (items[x].ToString().Substring(0, auxLength) == orderNo.Trim())
                                {
                                    string[] aux3 = items[x].ToString().Split(',');

                                    //Agregar el peso de las lineas que no tengan series
                                    if (aux3[4].ToString() == "")
                                    {
                                        netWeight += Convert.ToDecimal(aux3[5]);
                                    }
                                }
                            }

                            //Guardar en la tabla WhseReceiptGood
                            querySQL = new SqlCommand("insert into WhseReceiptGood(CreatedAt,CreatedBy,Product,NetWeight,GrossWeight,WhseReceipt,QtyBalance,CustomerOrderNo,InvoiceNo,ContainerQty,CommercialQty,ContainerUOM,CommercialUOM,ProdDescription) values(@CreatedAt,@CreatedBy,@Product,@NetWeight,@GrossWeight,@WhseReceipt,@QtyBalance,@CustomerOrderNo,@InvoiceNo,@QtyBalance,@QtyBalance,3,71,@ProdDescription); SELECT SCOPE_IDENTITY() as 'Id';", conexSQL);
                            querySQL.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                            querySQL.Parameters.AddWithValue("@CreatedBy", createdBy);
                            querySQL.Parameters.AddWithValue("@Product", productId);
                            querySQL.Parameters.AddWithValue("@NetWeight", netWeight);
                            querySQL.Parameters.AddWithValue("@GrossWeight", netWeight);
                            querySQL.Parameters.AddWithValue("@WhseReceipt", WhseReceiptId);
                            querySQL.Parameters.AddWithValue("@QtyBalance", qtyBalance);
                            querySQL.Parameters.AddWithValue("@CustomerOrderNo", orderNo);
                            querySQL.Parameters.AddWithValue("@InvoiceNo", invoice);
                            querySQL.Parameters.AddWithValue("@ProdDescription", prodDescription);
                            conexSQL.Open();
                            decimal WhseReceiptGoodId = (decimal)querySQL.ExecuteScalar();
                            conexSQL.Close();

                            //Guardar los series en WhseReceiptGoodReference
                            foreach (string s in serialArray)
                            {
                                if (s.Trim() != "")
                                {
                                    querySQL = new SqlCommand("insert into WhseReceiptGoodReference(CreatedAt,CreatedBy,WhseReceiptGood,Reference) values(@CreatedAt,@CreatedBy,@WhseReceiptGood,@Reference)", conexSQL);
                                    querySQL.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                                    querySQL.Parameters.AddWithValue("@CreatedBy", createdBy);
                                    querySQL.Parameters.AddWithValue("@WhseReceiptGood", WhseReceiptGoodId);
                                    querySQL.Parameters.AddWithValue("@Reference", s);
                                    conexSQL.Open();
                                    querySQL.ExecuteNonQuery();
                                    conexSQL.Close();
                                }
                            }
                        }
                    }
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "onload", "alert('ARCHIVO DE MANIFIESTO PROCESADO');", true);
                }
                catch (Exception ex)
                {
                    srSource.Close();
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "onload", "alert('Error: " + ex.Message + "');", true);
                }
            }

            if (rbConsignee.Checked)
            {
                if (!fuConsignee.HasFile)
                {
                    throw new ArgumentException("Necesita seleccionar un archivo.");
                }
                fuConsignee.SaveAs(MapPath("consignee.csv"));

                StreamReader srSource = new StreamReader(MapPath("consignee.csv"), Encoding.GetEncoding(1252));
                SqlConnection conexSQL = new SqlConnection(ConfigurationManager.AppSettings["DB"]);
                SqlCommand querySQL;
                string currentLine;
                string[] dataArray;
                string perlickSO, epicotSO, load_number, enter_date, pickup_date, delivery_date, pickup_city, delivery_name, delivery_address, delivery_city, delivery_state, delivery_postalcode, actual_pallets, actual_weight, description, pallet_spaces, mode, carrier_name;
                string aux = "0000000000" + txtReceiptNo.Text.Trim();
                string WhseReceiptNo = aux.Substring(aux.Trim().Length - 10);
                srSource.ReadLine();
                try
                {
                    while (srSource.Peek() != -1)
                    {
                        currentLine = srSource.ReadLine();
                        dataArray = currentLine.Replace("\"","").Split(',');
                        perlickSO = dataArray[0].Trim();
                        epicotSO = dataArray[1].Trim();
                        load_number = dataArray[2].Trim();
                        enter_date = dataArray[3].Trim();
                        pickup_date = dataArray[4].Trim();
                        delivery_date = dataArray[5].Trim();
                        pickup_city = dataArray[6].Trim();
                        delivery_name = dataArray[7].Trim();
                        delivery_address = dataArray[8].Trim();
                        delivery_city = dataArray[9].Trim();
                        delivery_state = dataArray[10].Trim();
                        delivery_postalcode = dataArray[11].Trim();
                        actual_pallets = dataArray[12].Trim();
                        actual_weight = dataArray[13].Trim();
                        description = dataArray[14].Trim();
                        pallet_spaces = dataArray[15].Trim();
                        mode = dataArray[16].Trim();
                        carrier_name = dataArray[17].Trim();

                        querySQL = new SqlCommand("declare @stateID int, @countryID int, @entityAddressID int, @entityID int, @keyNo varchar(8); " +
                                                "select top 1 @stateID=OID, @countryID=Country from State where Code = @code order by case when Country=2 then 1 when Country=37 then 2 else 3 end; " +
                                                "set @keyNo = (select top 1 right('000000' + cast(cast(KeyNo as int) +1 as varchar(8)),6) from EntityBase where objecttype=125 and keyno is not null and KeyNo like '0%' order by CreatedAt desc, KeyNo  desc); " +
                                                "insert into EntityAddress values(getdate(), @createdby, @street, null, @city, @stateID, @countryID, @zipcode, 0); select @entityAddressID = scope_identity(); " +
                                                "insert into EntityBase values(getdate(), @createdby, @keyNo, @name, null, null, @entityAddressID, null, 1, 0, 125, null, null); select @entityID = scope_identity(); " +
                                                "insert into WhseConsignee values(@entityID,null); " +
                                                "update G set G.RelWhseConsignee = @entityID from WhseReceipt R join WhseReceiptGood G on (G.WhseReceipt = R.OID) where R.WhseReceiptNo=@receiptNo and G.CustomerOrderNo = @orderNo;", conexSQL);
                        querySQL.Parameters.AddWithValue("@createdby", 3733);
                        querySQL.Parameters.AddWithValue("@street", delivery_address);
                        querySQL.Parameters.AddWithValue("@city", delivery_city);
                        querySQL.Parameters.AddWithValue("@zipcode", delivery_postalcode);
                        querySQL.Parameters.AddWithValue("@name", delivery_name);
                        querySQL.Parameters.AddWithValue("@orderNo", epicotSO);
                        querySQL.Parameters.AddWithValue("@code", delivery_state);
                        querySQL.Parameters.AddWithValue("@receiptNo", WhseReceiptNo);
                        conexSQL.Open();
                        querySQL.ExecuteNonQuery();
                        conexSQL.Close();
                    }
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "onload", "alert('ARCHIVO DE CONSIGNEES PROCESADO');", true);
                }
                catch(Exception ex)
                {
                    srSource.Close();
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "onload", "alert('Error: " + ex.Message + "');", true);
                }                
            }
        }

        protected void rbManifest_CheckedChanged(object sender, EventArgs e)
        {
            divPerlickManifest.Visible = true;
            divConsignee.Visible = false;
        }

        protected void rbConsignee_CheckedChanged(object sender, EventArgs e)
        {
            divConsignee.Visible = true;
            divPerlickManifest.Visible = false;            
        }
    }
}