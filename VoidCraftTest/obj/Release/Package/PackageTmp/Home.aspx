<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="VoidCraftTest.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h2>Home Page</h2>
        <asp:Label ID="Label1" runat="server" Text="Welcome to the site!"></asp:Label>
        <br />
        <br />
        <asp:HyperLink ID="Hyperlink1" runat="server" NavigateUrl="~/Login.aspx">Access Members Area</asp:HyperLink>
    </div>
    </form>
</body>
</html>
