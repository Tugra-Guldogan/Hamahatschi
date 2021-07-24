# Hamahatschi
Hamahatschi is a simple program to turn Hamachi and its service as well as network adapter on and off.

Usage
------

Simply compile the project or start the executable. It will try to find the adapter and the service and disable/enable Hamachi, depending on what its current state is. If it can't decide on what you want (because one thing is enabled and the other is disabled), it will prompt whether you want to start or close Hamachi.

Because I'm messing around with network adapters and services, this program requires elevated rights (i.e. UAC will bother you every time you start it).

If everything went according to plan, Hamachi should shut down/start up and the program will close itself. In case there was an error, the command line stays open and prints some information. If you are encountering an error and cannot solve it on your own, submit it as an issue. When opening an issue, please include everything Hamahatschi writes.

Motivation
----------

Because Steam's P2P functionality was severely broken for me and a few friends, we had to create direct connections between each other. We chose Hamachi for that as it was somewhat well established and known to all of us.

However, I disliked having Hamachi open 24/7, especially with its (more or less useless) service and network adapter that was only used from time to time. Hamahatschi was created as an attempt to solve this by disabling the network adapter Hamachi creates (called "Hamachi") and stopping its service ("LogMeIn Tunneling Engine"), therefore rendering it quite useless.
