using nanoFramework.M2Mqtt.Messages;
using nanoFramework.M2Mqtt;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;

namespace NFMqttClientApp
{
	public class Program
	{
		private const string c_SSID = "wifissid";
		private const string c_AP_PASSWORD = "wifipassword";

		private static MqttClient device = null;

		private const string c_DEVICEID = "deviceid";
		private const string c_USERNAME = c_DEVICEID;

		private const string c_BROKERHOSTNAME = "youreventgrid.westeurope-1.ts.eventgrid.azure.net";
		private const int c_PORT = 8883;
		private const string c_PUB_TOPIC = "message/fromnanoframework";
//		private const string c_PUB_TOPIC_LWT = "message/fromnanoframework/alert";
		private const string c_SUB_TOPIC = "message/device/#";

		public static void Main()
		{
			//// ROOT TLS cert is taken from DigiCert Global Root G3: https://www.digicert.com/kb/digicert-root-certificates.htm

			Debug.WriteLine("Hello from nanoFramework MQTT cLient!");

			// Wait for Wifi/network to connect (temp)
			SetupAndConnectNetwork();

			var caCert = new X509Certificate(ca_cert);
			var clientCert = new X509Certificate2(client_cert, client_key, string.Empty);
			device = new MqttClient(c_BROKERHOSTNAME, c_PORT, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

			device.ProtocolVersion = MqttProtocolVersion.Version_5;
			
			TryToConnect();

			device.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
			device.MqttMsgSubscribed += Client_MqttMsgSubscribed;
			device.ConnectionOpened += Client_ConnectionOpened;
			device.ConnectionClosed += Client_ConnectionClosed;
			device.ConnectionClosedRequest += Client_ConnectionClosedRequest;
			device.MqttMsgUnsubscribed += Client_MqttMsgUnsubscribed;

			device.Subscribe(new[] { c_SUB_TOPIC }, new[] { MqttQoSLevel.AtLeastOnce });

			var counter = 0;

			while (true)
			{
				string payload = $"{{\"counter\":{counter++}}}";
				var result = device.Publish(c_PUB_TOPIC, Encoding.UTF8.GetBytes(payload), "application/json; charset=utf-8", null);
				Debug.WriteLine($"Message sent ({result}): {payload}");
				Thread.Sleep(10000);
			}
		}

		private static void TryToConnect()
		{
			bool connected = false;

			while (!connected)
			{
				try
				{
					// Regular connect
					var resultConnect = device.Connect(c_DEVICEID, c_USERNAME, "");

					// Connect with Last will & testament. At this moment, willRetain is not supported yet by the EventGrid so keep that at 'false'.
//					var resultConnect = device.Connect(c_DEVICEID, c_USERNAME, "", false, MqttQoSLevel.AtLeastOnce, true, c_PUB_TOPIC_LWT, "unexpected disconnect", true, 30);

					if (resultConnect != MqttReasonCode.Success)
					{
						Debug.WriteLine($"MQTT ERROR connecting: {resultConnect}");
//						device.Disconnect();

						Thread.Sleep(1000);
					}
					else
					{
						Debug.WriteLine(">>> Device is connected");
						connected = true;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"MQTT ERROR Exception '{ex.Message}'");
//					device.Disconnect();

					Thread.Sleep(1000);
				}
			}
		}

		private static void Client_ConnectionClosedRequest(object sender, ConnectionClosedRequestEventArgs e)
		{
			Debug.WriteLine("Client_ConnectionClosedRequest");
		}

		private static void Client_ConnectionClosed(object sender, EventArgs e)
		{
			Debug.WriteLine("Client_ConnectionClosed");

//			TryToConnect();
		}

		private static void Client_ConnectionOpened(object sender, ConnectionOpenedEventArgs e)
		{
			Debug.WriteLine("Client_ConnectionOpened");
		}

		private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			Debug.WriteLine("Client_MqttMsgPublishReceived");

			Debug.WriteLine($"Topic: {e.Topic}");

			var message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

			Debug.WriteLine(message);
		}

		private static void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
		{
			Debug.WriteLine("Client_MqttMsgSubscribed");

			Debug.WriteLine($"Message identifier: {e.MessageId} of subscribed topic");
		}

		private static void Client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
		{
			Debug.WriteLine("Client_MqttMsgUnsubscribed");
		}

		public static void SetupAndConnectNetwork()
		{
			NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
			if (nis.Length > 0)
			{
				// get the first interface
				NetworkInterface ni = nis[0];

				if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
				{
					// network interface is Wi-Fi
					Debug.WriteLine("Network connection is: Wi-Fi");

					Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
					if (wc.Ssid != c_SSID && wc.Password != c_AP_PASSWORD)
					{
						// have to update Wi-Fi configuration
						wc.Ssid = c_SSID;
						wc.Password = c_AP_PASSWORD;
						wc.SaveConfiguration();
					}
					else
					{   // Wi-Fi configuration matches
					}
				}
				else
				{
					// network interface is Ethernet
					Debug.WriteLine("Network connection is: Ethernet");

					ni.EnableDhcp();
				}

				// wait for DHCP to complete
				WaitIP();
			}
			else
			{
				throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
			}
		}

		static void WaitIP()
		{
			Debug.WriteLine("Waiting for IP...");

			while (true)
			{
				NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
				if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
				{
					if (ni.IPv4Address[0] != '0')
					{
						Debug.WriteLine($"We have an IP: {ni.IPv4Address}");
						break;
					}
				}

				Thread.Sleep(500);
			}
		}

		#region Certificates (include BEGIN and END)
		private const string client_cert = @"-----BEGIN CERTIFICATE-----
cert
-----END CERTIFICATE-----";

		private const string client_key = @"-----BEGIN EC PRIVATE KEY-----
cert
-----END EC PRIVATE KEY-----";

		private const string ca_cert = @"-----BEGIN CERTIFICATE-----
cert
-----END CERTIFICATE-----";
		#endregion

	}
}