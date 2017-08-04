using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace PerlickManifest
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection conexSQL = new SqlConnection(@"Server=DESKTOP-D2S301D\SQL2016DEV;Database=Imexnet;Trusted_Connection=True");
            //SqlConnection conexSQL = new SqlConnection(@"Server=SERVER01;Database=Imexbk;User Id=sa;Password=Lozano901");
            SqlCommand querySQL;
            StreamReader srSource = new StreamReader("perlick.csv", Encoding.GetEncoding(1252));
            try
            {
                //Variables para cambiar cuando la aplicacion este en produccion
                string customerId = "3250";  //Cambiar el 3250 por el verdadero ID de Perlick
                string createdBy = "3733";   //Usuario de Gabriel. Confirmar si puede quedarse este ID
                string DEFAULT_PRODUCT_ID = "113458"; //Cambiarlo por el ID del producto default que se va a agregar para los casos donde no exista el producto. Revisar con Gabriel.


                //El numero de receipt se va a capturar en un textbox en pagina web
                string aux = "0000000000" + "1019";
                string WhseReceiptNo = aux.Substring(aux.Trim().Length - 10);
                string WhseReceiptId = "";
                string currentLine = "";
                string[] dataArray;
                ArrayList items = new ArrayList();

                //Variables para las columnas del archivo
                string load, partNo, orderNo, qtyBalance, model, serial, invoice;
                decimal netWeight;

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
                dataArray = currentLine.Split('|');
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
                    dataArray = currentLine.Split('|');
                    load = dataArray[0].Trim();
                    invoice = dataArray[1].Trim();
                    partNo = dataArray[3].Trim();
                    model = dataArray[5].Trim();
                    qtyBalance = dataArray[8].Trim();
                    netWeight = Convert.ToDecimal(dataArray[9].Trim());
                    serial = dataArray[10].Trim();
                    orderNo = dataArray[13].Trim();

                    bool existsMatch = false;
                    int index = -1;
                    foreach (string i in items)
                    {
                        index++;
                        string[] aux2 = i.Split('|');
                        if (orderNo.Trim() + "|" + partNo.Trim() == aux2[0] + "|" + aux2[1])
                        {
                            existsMatch = true;
                            break;
                        }
                    }
                    if (existsMatch)
                    {
                        string[] aux2 = items[index].ToString().Split('|');
                        items[index] = aux2[0].Trim() + "|" + aux2[1].Trim() + "|" + (Convert.ToInt16(aux2[2]) + 1).ToString() + "|" + aux2[3] + "|" + aux2[4] + (serial.Trim() != "" ? "," + serial : "") + "|" + (Convert.ToDecimal(aux2[5]) + Convert.ToDecimal(netWeight)).ToString();
                    }
                    else
                    {
                        items.Add(orderNo.Trim() + "|" + partNo + "|1|" + invoice + "|" + serial + "|" + netWeight.ToString());
                    }
                }
                srSource.Close();

                //Ordenar por el numero de orden
                items.Sort();

                foreach (string i in items)
                {
                    string[] aux2 = i.Split('|');
                    orderNo = aux2[0];
                    partNo = aux2[1];                    
                    qtyBalance = aux2[2];
                    invoice = aux2[3];
                    string[] serialArray = aux2[4].Split(',');
                    netWeight = Convert.ToDecimal(aux2[5]);

                    string productId = "";
                    //Obtener el ID del producto
                    querySQL = new SqlCommand("select top 1 OID from Product where ProductNo=@ProductNo and Customer=@Customer", conexSQL);
                    querySQL.Parameters.AddWithValue("@ProductNo", partNo);
                    querySQL.Parameters.AddWithValue("@Customer", customerId);
                    conexSQL.Open();
                    if (querySQL.ExecuteScalar() == null)
                    {
                        productId = DEFAULT_PRODUCT_ID;
                    }
                    else
                    {
                        productId = querySQL.ExecuteScalar().ToString();
                    }
                    conexSQL.Close();
                    
                    if (aux2[4].Trim() != "")
                    {
                        //Obtener la suma total por orden, para incluir el peso de los productos que no tienen una serie en el archivo
                        for (int x = 0; x < items.Count; x++)
                        {
                            int auxLength = orderNo.Trim().Length;
                            if (items[x].ToString().Substring(0, auxLength) == orderNo.Trim())
                            {
                                string[] aux3 = items[x].ToString().Split('|');

                                //Agregar el peso de las lineas que no tengan series
                                if (aux3[4].ToString() == "")
                                {
                                    netWeight += Convert.ToDecimal(aux3[5]);
                                }                                
                            }
                        }

                        //Guardar en la tabla WhseReceiptGood
                        querySQL = new SqlCommand("insert into WhseReceiptGood(CreatedAt,CreatedBy,Product,NetWeight,GrossWeight,WhseReceipt,QtyBalance,CustomerOrderNo,InvoiceNo) values(@CreatedAt,@CreatedBy,@Product,@NetWeight,@GrossWeight,@WhseReceipt,@QtyBalance,@CustomerOrderNo,@InvoiceNo); SELECT SCOPE_IDENTITY() as 'Id';", conexSQL);
                        querySQL.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                        querySQL.Parameters.AddWithValue("@CreatedBy", createdBy);
                        querySQL.Parameters.AddWithValue("@Product", productId);
                        querySQL.Parameters.AddWithValue("@NetWeight", netWeight);
                        querySQL.Parameters.AddWithValue("@GrossWeight", netWeight);
                        querySQL.Parameters.AddWithValue("@WhseReceipt", WhseReceiptId);
                        querySQL.Parameters.AddWithValue("@QtyBalance", qtyBalance);
                        querySQL.Parameters.AddWithValue("@CustomerOrderNo", orderNo);
                        querySQL.Parameters.AddWithValue("@InvoiceNo", invoice);
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
                Console.WriteLine("ARCHIVO PROCESADO");
                Console.Read();
            }
            catch (Exception ex)
            {
                srSource.Close();
                Console.WriteLine("Error: " + ex.Message);
                Console.Read();
            }
        }
    }
}
