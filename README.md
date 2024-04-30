# .NET nanoframework Cloud-MQTT-Broker-support

.NET nanoframework demonstration as a client for Azure EventGrid support for MQTT 

## Introduction

this Visual Studio project demonstrates how to connect to the EventGrid cloud MQTT broker for:
- Connect
- Send data to a topic
- Receive data on a topic

If everything is set up correctly, you get a flow like:

![image](https://github.com/sandervandevelde/Nanoframework-Cloud-MQTT-Broker-support/assets/694737/e2d723ca-5da6-4243-b136-852aa5056519)


## Certificates

You need three certificates:

1. The public TLS certificated for the connection to the broker based on [DigiCert Global Root G3](https://www.digicert.com/kb/digicert-root-certificates.htm)
2. Private client certificate
3. Public client certificate 

## Last will and testament

This new EventGrid namespace feature is now demonstrated too (you need to update your m2mqtt library if needed).

Notice the willRetain flag is set to false. That feature is not supported yet by the EventGrid namespace.

## Links

A demonstration of how to set up the MQTT Broker in Event Grid is seen [here](https://sandervandevelde.wordpress.com/2023/10/14/a-first-look-at-azure-eventgrid-mqtt-support/)

## Credits

This [post](https://stackoverflow.com/questions/78314752/how-to-use-event-grid-namespace-mqtt-hostname-on-esp32-or-esp32-online-like-wokw/78330697#78330697) from *Roman Kiss* helped me with the client credentials. 
