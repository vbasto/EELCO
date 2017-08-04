<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="PerlickManifestWeb.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge;" />
    <title>EELCO Perlick Manifest</title>
    <link href="lib/bootstrap/v3.3.6/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="lib/jqwidgets/css/jqx.base.css" rel="stylesheet"/>
    <script src="lib/jquery/v2.2.0/jquery-2.2.0.min.js"></script>
    <script src="lib/bootstrap/v3.3.6/js/bootstrap.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">
            <div class="row form-group eelco-header-style">
                <div class="col-lg-1 col-md-1 col-sm-1 col-xs-1" style="height:80px">
                    <img src="img/logo_eelco.png" style="height:70px"/>
                </div>
                <div class="col-lg-10 col-md-10 col-sm-10 col-xs-10 text-center">
                    <h3 style="color:white; font-family:Aharoni; font-weight:bold">PERLICK MANIFEST</h3>
                </div>
            </div>
            <div class="row" style="margin-left:15px">
                <asp:Label ID="lblReceiptNo" runat="server" Text="Enter the Receipt Number:"></asp:Label>
                <asp:TextBox ID="txtReceiptNo" runat="server"></asp:TextBox>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:RadioButton ID="rbManifest" runat="server" GroupName="file" AutoPostBack="true" OnCheckedChanged="rbManifest_CheckedChanged" /> Manifest
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:RadioButton ID="rbConsignee" runat="server" GroupName="file" AutoPostBack="true" OnCheckedChanged="rbConsignee_CheckedChanged" /> Consignee
            </div>
            <br /><br />
            <div id="divPerlickManifest" runat="server" class="row" style="margin-left:15px">
                <asp:Label ID="lblFileSelection" runat="server" Text="Select Manifest File:"></asp:Label>
                <asp:FileUpload ID="fuPerlickFile" runat="server" />
                <br />
                <asp:Button ID="btProcess" runat="server" CssClass="btn btn-primary" Text="Process File" OnClick="btProcess_Click" OnClientClick="return confirm('Do you want to process the Manifest file?');" />
            </div>
            <div id="divConsignee" runat="server" class="row" style="margin-left:15px">
                <asp:Label ID="Label1" runat="server" Text="Select Consignee File:"></asp:Label>
                <asp:FileUpload ID="fuConsignee" runat="server" />
                <br />
                <asp:Button ID="btConsignee" runat="server" CssClass="btn btn-primary" Text="Process File" OnClick="btProcess_Click" OnClientClick="return confirm('Do you want to process the Consignee file?');" />
            </div>
        </div>
    </form>
    <link href="css/index.css" rel="stylesheet" />
</body>
</html>
