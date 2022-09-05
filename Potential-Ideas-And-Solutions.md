Issues:

* Server - how to keep track of which client is which
	* Solutions:
		* Use server property of being a round-robin to calculate which client is which.
		* Ask Client to re-send messages it's not supposed to get.
		* Use Router socket-type. Maybe 0MQ handles this automatically?

* Client - needs to keep track of server id to know which server it's connected to and should recieve replies from.
	* Solutions:
		*