<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Reporteador.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="X-UA-Compatible" content="IE=edge;" />
    <title>EELCO Reporting</title>
    <link href="lib/bootstrap/v3.3.6/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="lib/jqwidgets/css/jqx.base.css" rel="stylesheet"/>
    <link href="lib/jquery/v2.2.0/jquery-ui.min.css" rel="stylesheet"/>
    <link href="css/index.css" rel="stylesheet" />
    <script src="lib/jquery/v2.2.0/jquery-2.2.0.min.js"></script>
    <script src="lib/jquery/v2.2.0/jquery-ui.min.js"></script>
    <script src="lib/bootstrap/v3.3.6/js/bootstrap.min.js"></script>
    <script>
        $(function () {
            var pos = 1;
            if (localStorage.getItem("currentPosition") != null) {
                pos = localStorage.getItem("currentPosition");
            }
            $("#accordion").accordion({
                collapsible: true,
                active: parseInt(pos)
            });
        });

        SaveAccordionPosition = function (pos) {
            localStorage.setItem("currentPosition", pos);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">
            <div class="row form-group eelco-header-style">
                <div class="col-lg-1 col-md-1 col-sm-1 col-xs-1" style="height:80px">
                    <img src="img/logo_eelco.png" style="height:70px"/>
                </div>
                <div class="col-lg-10 col-md-10 col-sm-10 col-xs-10 text-center">
                    <h3 style="color:white; font-family:Aharoni; font-weight:bold">REPORTING</h3>
                </div>
            </div>
        </div>
        <div class="col-lg-12 col-md-12 col-sm-12">
            <div class="col-lg-3 col-md-3 col-sm-3 col-xs-3">
                <div id="accordion" style="width: 270px">
                    <h3>Core</h3>
                    <div>
                        <button>Traffic</button>
                        <button>Customers</button>
                        <button>Importer & Consignee</button>
                        <button>Carriers</button>
                        <button>Product Profile</button>
                        <button>Quotes</button>
                        <button>Shipper</button>
                    </div>
                    <h3>US Customs Clearance</h3>
                    <div>
                    </div>
                    <h3>Utilities</h3>
                    <div>
                    </div>
                    <h3>Logistics</h3>
                    <div>
                        <asp:Button id="btReceipts" runat="server" Text="WReceipts" CssClass="btn btn-primary" Width="200px" OnClientClick="SaveAccordionPosition(3);" OnClick="btReceipts_Click" />
                        <button>BOL's</button>
                        <button>Freight Loads</button>
                        <button>WReceipts</button>
                        <button>Whse Picks</button>
                        <button>WDispatches</button>
                        <button>Whse Inventory</button>
                        <button>WLocations</button>
                        <button>WConsignee</button>
                    </div>
                    <h3>Accounting</h3>
                    <div>
                    </div>
                </div>
            </div>
            <div class="col-lg-9 col-md-9 col-sm-9 col-xs-9">
                <label>Select which fields must be included in the report:</label>
                <br /><br />
                <asp:CheckBoxList ID="cblColumns" runat="server" RepeatDirection="Vertical" RepeatColumns="5" AutoPostBack="true"></asp:CheckBoxList>
                <br /><br />
                <asp:Button ID="btPreview" runat="server" Text="Preview" CssClass="btn btn-primary" OnClick="btPreview_Click" />
            </div>
        </div>
    </form>
</body>
    
</html>
