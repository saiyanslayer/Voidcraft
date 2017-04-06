<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="VoidCraftTest.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
    </style>
    <link href="LoginStyle.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server" class="box">
    <div class="content">
    <h1>LogIn Page</h1>
        <asp:Label ID="Label1" runat="server" Text="Please log in below to access the membership area"></asp:Label>
        <br />
        <br />

        <asp:Login ID="LoginControl" runat="server" onauthenticate="LoginControl_Authenticate" CssClass="content">
            <LayoutTemplate>          
                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="LoginControl">*</asp:RequiredFieldValidator>
                <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label>
                <asp:TextBox ID="UserName" runat="server" CssClass="field"></asp:TextBox>
                <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="LoginControl">*</asp:RequiredFieldValidator>
                <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label>
                <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="field"></asp:TextBox>
                <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                <asp:CheckBox ID="RememberMe" runat="server" Text="Remember me next time." />
                <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Log In" ValidationGroup="LoginControl" CssClass="btn" />
            </LayoutTemplate>

        </asp:Login>
    </div>
    </form>

</body>
</html>
