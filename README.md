# Minimal Unity Netcode Project
A minimal project for testing multiplayer connection over IP with Unity's Netcode for GameObjects.

The intended audience for Minimal Unity Netcode Project (MUNP) is people who want to get started with multiplayer in Unity. When starting out, there are a lot of problems which can arise when following the [official Netcode tutorials](https://docs-multiplayer.unity3d.com/netcode/current/about/). The purpose of this project is to minimize the number of things which can go wrong, while still being able to test the basic use cases of online multiplayer connections. That is, before we can start iterating actual multiplayer gameplay, we have to connect clients and hosts over WAN and LAN.

Index:
* [Connecting over IP](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#connecting-over-ip)
* [Use Cases](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#use-cases)
  * [Localhost to Localhost](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#localhost-to-localhost)
  * [Computer A on Lan 1 to computer A on Lan 1](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#computer-a-on-lan-1-to-computer-a-on-lan-1)
  * [Computer A on Lan 1 to computer B on Lan 1](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#computer-a-on-lan-1-to-computer-b-on-lan-1)
  * [Computer A on Lan 1 to computer B on Lan 2](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#computer-a-on-lan-1-to-computer-b-on-lan-2)
* [Technical notes on MUNP](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#technical-notes-on-munp)
* [Tools for debugging](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#tools-for-debugging)

## Connecting over IP
![one does not simply connect over ip](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/7f7f6cb1-ef0b-41cc-930b-ab5cccf9ceed)

The purpose of connecting host and client is to send data packets back and forth. (See [OSI Model](https://en.wikipedia.org/wiki/OSI_model) and e.g. [Transport Layer](https://en.wikipedia.org/wiki/Transport_layer) for more theoretical background.)

To illustrate what can go wrong, let's follow a data packet's example journey:

0. Computer A on LAN 1  ([Local Area Network](https://en.wikipedia.org/wiki/Local_area_network)) wants to establish a connection over the internet with computer B on LAN 2 to play a multiplayer game.
1. Computer A sends a relevant data packet through its firewall.
2. The firewall must have an outbound rule for the packet's destination port X, destination IP Y, and protocol Z (TCP or UDP).
3. The packet is sent out through a relevant interface (e.g. wifi/ethernet with LAN IP 192.168.m.n) to a connected router (usually with address 192.168.0.1 or 192.168.1.1).
4. At your router, the packet is redirected (or routed) to its destination IP (possibly after passing rules in a firewall at the router).
   - The router sends the packet through its WAN facing interface. WAN or [Wide Area Network](https://en.wikipedia.org/wiki/Wide_area_network) addresses are also known as remote addresses or public IPs. The worlds largest WAN is the Internet.
   - Your router's remote address R is automatically provided by your ISP ([Internet Service Provider](https://en.wikipedia.org/wiki/Internet_service_provider)).
5. Your ISP routes the packet to the receiving router at remote address Y (possibly after passing rules in a firewall at the receiving router, or even the ISP).
6. At the receiving router there is now a problem. The packet is addressed to Y... but the target computer B on LAN 2 has a LAN IP "L", not a remote address. This is where NAT ([Network Address Translation](https://en.wikipedia.org/wiki/Network_address_translation)) comes in, a simple form of which is [port forwarding](https://en.wikipedia.org/wiki/Port_forwarding).
   - Check out e.g. [www.portforward.com](https://portforward.com/) for step by step guides with pictures on how to port forward your specific router.
   - By having set up a port forwarding rule in the receiving router for port X, the packets destination IP is changed (translated) to L from Y. The packet is then routed to L.   
7. At computer B on LAN 2 the packet must pass computer B's firewall. The firewall must have an inbound rule for the packet's port X, destination IP L, and protocol Z (TCP or UDP).
8. Finally, the multiplayer game on computer B expecting packets on port X, destination IP L, and protocol Z (TCP or UDP) can receive the data. 

_Notes on return packages:_
* When computer B responds with packets to A, it may not be necessary to have outbound/inbound firewall rules for the return packet if the [firewalls are stateful](https://en.wikipedia.org/wiki/Stateful_firewall) (e.g. Microsoft Defender Firewall). Thus firewall rules are usually only necessary for establishing connections rather than per package.
* The routers handling return packages are as well likely stateful and will not need explicit port forwarding for response packages, but will have kept track of ports and addresses used to identify a packet as being a response.

_Notes on NAT:_
* In the same way your computer is usually given a LAN IP behind a router, your router may itself have received a LAN IP from your ISP behind another router. This is called [Carrier Grade NAT](https://en.wikipedia.org/wiki/Carrier-grade_NAT). Without a public IP, you cannot directly host over WAN, as there is no IP for clients to connect to. There are workarounds in the form of connecting to a server which has a public IP and then bouncing traffic via that server. Depending on your ISP you might be able to receive a public IP with more or less hassle.

As can be seen, there are a lot of things which have to match up and plenty of ways for a packet to become dropped, having nowhere to go. 

Sort of like a very pedantic postal service.

## Use Cases

Below are listed a few common use cases and related problems which may arise. The purpose of MUNP is to test these use cases to see if connection is successful. To use MUNP for testing, build MUNP or get the latest build [from relesaes](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/releases), then run the .exe file to start an instance of MUNP.

### Localhost to Localhost

The simplest connection which "should work". 

Looking back at the example packet journey in [connecting over IP](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#connecting-over-ip) the packet never leaves computer A at step 3, but instead is sent to through the loopback interface (a.k.a. Localhost) on [reserved IP](https://en.wikipedia.org/wiki/Reserved_IP_addresses) 127.0.0.1. The packet is then looped back to the destination program at port X.

To test that this works with MUNP, do the following.

1. Computer A starts two instances of MUNP, instance a and b.
2. Instance a enters IP 127.0.0.1 and clicks Listen Server. A cube should appear in the window.
3. Instance b enters IP 127.0.0.1 and clicks client. Another cube should spawn, being rocketed away due to rigidbody collision.

![MUNP Listen Server host](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/634fd191-5fbe-4019-ae4c-1f60582c2de0)
![MUNP Listen Server client](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/beb8bbca-1c79-4e0f-82fb-62bafc7f3b97)

If this does not work then...
* You might have typed in the wrong address.
* You might have a firewall rule blocking localhost traffic (or not allowing it).
* You might have a bad netcode version, see [technical notes on MUNP](https://github.com/jkvastad/Minimal-Unity-Netcode-Project#technical-notes-on-munp).

### Computer A on Lan 1 to computer A on Lan 1

Similar to localhost-to-localhost, but using computer A's LAN IP. On e.g. Windows this can be found by running the command ipconfig in the command prompt. 

To test that this works with MUNP, do the following.

1. Computer A starts two instances of MUNP, instance a and b.
2. Instance a enters IP e.g. 192.168.m.n and clicks Listen Server. A cube should appear in the window.
3. Instance b enters IP e.g. 192.168.m.n and clicks Client. Another cube should spawn, being rocketed away due to rigidbody collision.

Note that even though both this and the localhost to localhost scenario have packets arriving at computer A sent from computer A, it is not possible to mix-and-match IPs. If you host at localhost and send to IP 192.168.m.n or vice versa the packets will be dropped.

### Computer A on Lan 1 to computer B on Lan 1

If the destination is on LAN 1 the packet is routed to destination LAN IP Y at step 4 in the example packet journey, instead of going through the routers remote interface. The packet then arrives at computer B as in step 7.

To test that this works with MUNP, do the following.

1. Computer A starts an instances of MUNP, instance a.
2. Computer B starts an instances of MUNP, instance b.
3. Instance a enters its LAN IP e.g. 192.168.m.n and clicks Listen Server. A cube should appear in the window.
   - Computer A must make sure there is an inbound firewall rule allowing TCP and UDP traffic on port 7777 to reach its IP.
5. Instance b enters computer A's LAN IP e.g. 192.168.m.n and clicks Client. Another cube should spawn, being rocketed away due to rigidbody collision.
   - Computer B must make sure there is an outbound firewall rule allowing TCP and UDP traffic on port 7777 to reach the IP of computer A.
  
### Computer A on Lan 1 to computer B on Lan 2

This is the scenario described in the example packet journey

To test that this works with MUNP, do the following.

1. Computer A starts an instances of MUNP, instance a.
2. Computer B starts an instances of MUNP, instance b.
3. Instance a enters its LAN IP e.g. 192.168.m.n and clicks Listen Server. A cube should appear in the window.
   - Computer A must make sure there is an inbound firewall rule allowing TCP and UDP traffic on port 7777 to reach its IP.
   - The router connected to computer A must have a port forwarding rule routing packets arriving for port 7777 to A's LAN IP.
5. Instance b enters A's routers remote address IP e.g. 123.1.2.3 and clicks Client. The rmote address may be found by searching e.g. "What is my ip". After clicking Client another cube should spawn, being rocketed away due to rigidbody collision.
   - Computer B must make sure there is an outbound firewall rule allowing TCP and UDP traffic on port 7777 to reach the IP of computer A.
  
Of note is that if you try using computer A and LAN 1 as computer B and LAN 2 there is a high likelihood of failure: In theory you should be able to ask your router to send a packet to its own remote address and then treat is as if it arrived over the internet. This feature is called [NAT hairpinning](https://en.wikipedia.org/wiki/Network_address_translation#NAT_hairpinning) and is not supported by all routers.

## Technical notes on MUNP

* MUNP can host a listen server (server + client) to which clients may connect.
* Unity (and other game services such as Steam and Unreal) [use port 7777 as default](https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers#Registered_ports).
* Since MUNP is purposefully made to be minimal, there is no error handling for bad connections. Think of each MUNP instance as a one-shot program, if something goes wrong just close and restart.
* Follow the "step by step project recreation.txt" if you want to go through creating MUNP manually.
* MUNP is made for Unity 2022.2.1f1 using Netcode 1.1.0. Be wary that there is no guarantee untested Netcode packages actually work - a hard to spot problem. You can always make your own MUNP to check a specific configuration.
* If on Windows and the screen size in MUNP is not the one specified in Unity's player settings you need to manually delete the registry keys storing the window size, using window's Registry Editor. See [peter77's Unity Forum answer](https://forum.unity.com/threads/default-screen-dimensions-being-ignored-after-build.500178/):
  * ```
    These keys are located under:
    Computer\HKEY_CURRENT_USER\Software\<Company Name>\<Product Name>\
    Where <Company Name> and <Product Name> must be replaced with whatever you entered for those in the PlayerSettings.
    The keys you want to remove are:
    	Screenmanager Resolution Height
    	Screenmanager Resolution Width
    	Screenmanager Is Fullscreen mode
    ```

## Tools for debugging
A very useful tool for debugging is [Wireshark](https://www.wireshark.org/). Wireshark is a network diagnostics tool for looking inside network interfaces, such as the loopback interface (127.0.0.1/localhost) and ethernet/wifi connections. Use e.g. filter `udp.port == 7777 || tcp.port == 7777` to filter for packets sent to Unity's default port. This can help identify if packets are arriving at all. Another source of information is the Unity player log. Different locations for the [log file are listed here](https://docs.unity3d.com/Manual/LogFiles.html).
