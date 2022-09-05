import zmq
import json
import time

# Basic client which connects to tinyqueue webserver, prompts the user for input, sends a request to enter queue and sends a subscribe request. Queue is then constantly updated.


context = zmq.Context()
# Socket to talk to server
print("Connecting to server...")
socket = context.socket(zmq.DEALER)

if socket.connect("tcp://tinyqueue.cognitionreversed.com:5556"):

	queueName = input("Enter your Queue Alias: ")

	ticket = {'enterQueue':True, 'name':queueName}
	socket.send_json(ticket)
	message = socket.recv_json()

	if 'ticket' and 'name' in message:
		print(message['ticket'])
		print(message['name'])


	subReq = {'subscribe':True}
	socket.send_json(subReq)

	while True:

		message = socket.recv_json()

		if 'queue' and 'supervisors' in message:
			templist = str(message['queue']).split(',')
			for i in templist:
				print(i + "\n")

		if not message:
			socket.send_json({''})

	#sub = {'subscribe':True}



# Do 10 requests, waiting each time for response
#for request in range(10):
#	print(f"Sending request {request}...")
#	socket.send_string("Hello")

	# Get reply
#	message = socket.recv()
#	print(f"Receive reply {request} [ {message} ]")