TODO: add release distributable
# Minimal Unity Netcode Project
A minimal project for testing multiplayer connection over IP with Unity's Netcode for GameObjects.

The intended audience for Minimal Unity Netcode Project (MUNP) is people who want to get started with multiplayer in Unity. When starting out, there are a lot of problems which can arise when following the [official Netcode tutorials](https://docs-multiplayer.unity3d.com/netcode/current/about/). The purpose of this project is to minimize the number of things which can go wrong, while still being able to test the basic use cases of online multiplayer connections. That is, before we can start iterating actual multiplayer gameplay, we have to connect clients and hosts over WAN and LAN.

## Connecting over IP
![one does not simply connect over ip](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/7f7f6cb1-ef0b-41cc-930b-ab5cccf9ceed)

The purpose of connecting host and client is to send data packets back and forth. (See [OSI Model](https://en.wikipedia.org/wiki/OSI_model) and e.g. [Transport Layer](https://en.wikipedia.org/wiki/Transport_layer) for more theoretical background.)

To illustrate what can go wrong, let's follow a data packet's example journey:

0. Computer A on LAN 1  ([Local Area Network](https://en.wikipedia.org/wiki/Local_area_network)) wants to establish a connection over IP with computer B, possibly on LAN 2, to play a multiplayer game.
1. Computer A sends a relevant data packet through its firewall.
2. The firewall must have an outbound rule for the packet's destination port X, destination IP Y, and protocol Z (TCP or UDP).
3. The packet is sent out through a relevant interface (e.g. wifi/ethernet with LAN IP 192.168.m.n).
   - If the destination is computer A itself, the packet is sent through the loopback interface (a.k.a. Localhost) on [reserved IP](https://en.wikipedia.org/wiki/Reserved_IP_addresses) 127.0.0.1. The packet is then looped back to the destination program at port X.
   - If the destination is LAN or WAN ([Wide Area Network](https://en.wikipedia.org/wiki/Wide_area_network), the largest of which is the Internet) it is sent to a connected router (usually with address 192.168.0.1 or 192.168.1.1)
4. At your router, the packet is redirected (or routed) to its destination IP (possibly after passing rules in a firewall at the router).
   - If the destination is on LAN 1 the packet is routed to destination IP Y (this skips to step 7).
   - If the destination is a WAN address (a.k.a. remote address or public IP) the packet is sent via your router's remote address R, an address automatically provided by your ISP ([Internet Service Provider](https://en.wikipedia.org/wiki/Internet_service_provider)).
5. Your ISP routes the packet to the receiving router at remote address Y (possibly after passing rules in a firewall at the receiving router).
6. At the receiving router there is now a problem. The packet is addressed to Y... but the target computer B on LAN 2 has a LAN IP "L", not a remote address. This is where NAT ([Network Address Translation](https://en.wikipedia.org/wiki/Network_address_translation)) comes in, a simple form of which is [port forwarding](https://en.wikipedia.org/wiki/Port_forwarding). By having set up a port forwarding rule in the router for port X, the packets destination IP is changed (translated) to L from Y. The packet is then routed to L.
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

Below are listed a few common use cases and related problems which may arise. The purpose of MUNP is to test these use cases to see if connection is successful. To use MUNP for testing, build MUNP or get the build file here **TODO: add link**, then run the .exe file to start an instance of MUNP.

### Localhost to Localhost

The simplest connection which "should work". 

1. Computer A starts two instances of MUNP, instance a and b.
2. Instance a enters IP 127.0.0.1 and clicks Listen Server. A cube should appear in the window.
3. Instance b enters IP 127.0.0.1 and clicks client. Another cube should spawn, being rocketed away due to rigidbody collision.

![MUNP Listen Server host](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/634fd191-5fbe-4019-ae4c-1f60582c2de0)
![MUNP Listen Server client](https://github.com/jkvastad/Minimal-Unity-Netcode-Project/assets/9295196/beb8bbca-1c79-4e0f-82fb-62bafc7f3b97)

### Computer A on Lan 1 to computer A on Lan 1

Similar to localhost, but using computer A's LAN IP. On e.g. Windows this can be found by running the command ipconfig in the command prompt. 

* If you host on computer A localhost, you cannot connect to computer A on A's LAN IP and vice versa - the IPs must match.


## Technical notes on MUNP

* MUNP can host a listen server (server + client) to which clients may connect.
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

Tools for debugging:
Wireshark - network diagnostics tool for looking inside network interfaces, such as the loopback interface (127.0.0.1/localhost) and ethernet/wifi connections. Use e.g. filter "udp.port == 7777 || tcp.port == 7777" to filter for packets sent to Unity's default port.

TODO: Lista alla usecases: 
localhost <-> localhost
LAN comp A <-> LAN comp A
LAN comp A <-> LAN comp B
WAN comp A <-> LAN comp A
WAN comp A <-> LAN comp B
WAN comp A <-> LAN comp A + B (samma som LAN comp A <-> LAN comp A + WAN comp A <-> LAN comp B?)
WAN comp A <-> LAN comp B + C + etc.

Three main use cases to try:
Local testing - testing on your own machine
Host one instance at 127.0.0.1 and connect to 127.0.0.1

LAN testing - testing on your Local Area Network (your router network, a.k.a. "LAN")
Host one instance at your computers LAN address, found by running e.g. "ipconfig" in the command prompt in Windows. This will be e.g. "192.168.x.y". Client connects to "192.168.x.y". Since data packets are now leaving your computer, this will require allowing those packets through your firewall. Default port for packets in Unity is 7777 (see your Unity Transport component). Theoretically, the client will need an outgoing rule allowing for port 7777 since the client is initializing the connection to the host, and the host will need an incoming rule allowing port 7777 since it is receiving the connection request. Search online for opening ports in your firewall, e.g. for Windows Defender there is at least https://www.wikihow.com/Open-Ports and https://www.howtogeek.com/394735/how-do-i-open-a-port-on-windows-firewall/.

Try it using the same computer as host/client (does not work with Netcode 1.0, gives ICMP error port unreachable., and two different computers on your LAN, one being the host and the other being the client.

WAN testing - testing over the internet (router to router, a.k.a. actual online multiplayer)
Host one instance at your computers WAN address, found by going to e.g. "https://www.whatismyip.com/". This will be e.g. "123.456.987.654". Client connects to e.g. "123.456.987.654". This will require port forwaring on the hosts router as a packet arriving at the roters external/outward facing IP "123.456.987.654" will not know what the internal LAN IP of the host is. Using port forwarding we can make a rule inside the router that e.g. all packets going to port 7777 should be sent to address "192.168.x.y" (This is a form of NAT https://en.wikipedia.org/wiki/Network_address_translation). Check out https://portforward.com/ for guides on how to forward your specific router since each router has different features and runs different software. You can go to your router using your browser by typing in it's IP, the default IP is usually printed on the physical router, or you can try the usual IPs which are 192.168.0.1 and 192.168.1.1.

TODO: NAT hairpinning problems, use mobile hotspot wifi to test WAN (make sure mobile is not using home router for its internet, check that the IPs are different using whatismyip).
TODO: Multiple machines behind same LAN in same WAN game

Notable pitfalls:
*The host IP must match the client IP
	This means if you host the game at the default IP 127.0.0.1, a computer on LAN or WAN will not be able to connect.
