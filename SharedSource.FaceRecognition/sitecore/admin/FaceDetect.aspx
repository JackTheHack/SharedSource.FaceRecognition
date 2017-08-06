<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FaceDetect.aspx.cs" Inherits="SharedSource.FaceRecognition.sitecore.admin.FaceDetect" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div>
        <asp:Button runat="server" id="btnDetectFaces" OnClick="btnDetectFaces_OnClick" text="Train Dataset"/>
            </div>
        <div>
        <asp:Button runat="server" text="Training Status" OnClick="btnTrainingStatus_OnClick" id="btnTrainingStatus"/>        
            <asp:CheckBox runat="server" Text="Force" Checked="False" id="chkForce"/>
            </div>
        <div>
        <asp:Label runat="server" id="lblResult"></asp:Label>
            </div>
        <div>
            <asp:TextBox TextMode="MultiLine" style="width:100%" runat="server" ID="txtLog"></asp:TextBox>
            </div>
    </div>
    </form>
</body>
</html>
