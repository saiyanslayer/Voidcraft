<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Member.aspx.cs" Inherits="VoidCraftTest.Member.Member" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="../StyleSheet2.css" rel="stylesheet" type="text/css" />
</head>
    
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"> </asp:ScriptManager>

        <!-- ModalPopUpExtender -->
        <cc1:ModalPopUpExtender ID="BuildPopUp" runat="server" 
            TargetControlID="PopUpButton" 
            PopupControlID="BuildPopUpPanel" 
            EnableViewState="false" 
            CancelControlID="CancelButton" 
            BackgroundCssClass="modalBackground"
            />
            
        <asp:Panel ID="BuildPopUpPanel" runat="server" HorizontalAlign="Center" CssClass="modalPopup">
             <asp:Label ID="BuildPopUpLabel" runat="server" Text="This is a popup without formatting" />
            <br />
            <asp:Button ID="BuildButton" runat="server" Text="Build!" OnClick="BuildRecipeButton_Click" />
            <asp:Button ID="CancelButton" runat="server" Text="Canel"/>
        </asp:Panel>
        <asp:Button ID="PopUpButton" runat="server" Style="display:none;"/>
        <!-- ModalPopUpExtender -->



        <ul class="MenuBar">
            <li class="MenuBar-ListItem"><a class="MenuBar-Link" href="Member.aspx">HOME</a></li>
            <li class="MenuBar-ListItem">RESOURCES</li>
            
            <li class="MenuBar-ListItemRight"><asp:LoginStatus ID="LoginStatus" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/home.aspx" class="MenuBar-Link"/></li>
            <li class="MenuBar-ListItemRight"><asp:LoginName ID="LoginName" runat="server" /></li>
        </ul>
    <div class="clearfix">
        <div class="UserStuff Column" id="leftsidebar">
            <h3>RESOURCES</h3>
                <asp:Table ID="ResourceTable" runat="server" Width="100%" Margin="2px">
                    <asp:TableRow>
                        <asp:TableCell Width="40px" />
                        <asp:TableCell Width="200px" />
                    </asp:TableRow>
                </asp:Table>
                <asp:Panel ID="WorkerPanel" runat="server"/>
        </div>
        <div id="rightsidebar" class="Column" style="width:75%;">
            <h2>Member's Area</h2>
            <asp:Label ID="Label1" runat="server" Text="Welcome to the Member's Area!" />
            <br />
            <h3>Pending Events</h3>
            <asp:Table ID="EventsTable" runat="server" Width="100%">
                <asp:TableRow>
                    <asp:TableCell Font-Underline="true">Event ID</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Type</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Notes</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Time Left</asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <br />
            <h3>Create Stuff</h3>
            <asp:UpdatePanel ID="CreateUpdatePanel" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="PopupPanel" runat="server" Visible="false">
                        Here is the popup
                    </asp:Panel>
                    
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Table ID="RecipeTable" runat="server" Width="100%">
                <asp:TableRow>
                    <asp:TableCell Font-Underline="true">Recipe ID</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Name</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Resources Needed</asp:TableCell>
                    <asp:TableCell Font-Underline="true">Time Required</asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </div>
        </div>
    </form>
</body>
</html>
