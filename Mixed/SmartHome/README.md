SmartHome - Xamarin.Forms + HoloLens
=======
The sample consists of two applications:
1. Scanner - HoloLens-based application, scans environment and sends it to the client app.
2. Client - Xamarin.Forms application for Android, iOS and UWP. Displays Scanner's data.

The connection between both apps is established using TCP\IP where the Client is the host and shares
it's IP via QR code. Scanner continuously sends spatial mapping data. Also, it allows 
user to manually mark smart devices (e.g. wi-fi bulbs) in order to be able to control them in the Client app. 

![Screenshot](Screenshots/Screenshot.gif)

Full video is here: [https://www.youtube.com/watch?v=OnDbRScYuCo](https://www.youtube.com/watch?v=OnDbRScYuCo)

![Screenshot](Screenshots/Screenshot2.png)

Third Party Libraries:
=======
- [Sockets Plugin for Xamarin](https://github.com/rdavisau/sockets-for-pcl) - TCP/IP transport
- [protobuf-net](https://github.com/mgravell/protobuf-net) - Fast binary serialization
- [ZXing.Net for Xamarin.Forms](https://github.com/Redth/ZXing.Net.Mobile) - QR code generation & recognition
- [LifxHttpNet](https://github.com/mensly/LifxHttpNet) - Lifx (wi-fi bulbs) client

