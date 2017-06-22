<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FaceDetect.aspx.cs" Inherits="SharedSource.FaceRecognition.sitecore.admin.FaceDetect" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button runat="server" id="btnDetectFaces" OnClick="btnDetectFaces_OnClick" text="Detect Faces"/>
    </div>
    </form>
</body>
</html>
