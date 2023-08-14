TODO: Proper markup and pictures
TODO: note where player log is located
TODO: netcode 1.4.0 is broken? Fails to connect with ICMP port unreachable, but works with Netcode 1.1.0

A minimal project for testing multiplayer connection over IP with Unity's Netcode for GameObjects.

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