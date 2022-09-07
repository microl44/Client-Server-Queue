import zmq
import json
import time

# Basic client which connects to tinyqueue webserver, prompts the user for input, sends a request to enter queue and sends a subscribe request. Queue is then constantly updated.


context = zmq.Context()
# Socket to talk to server
print("Connecting to server...")
socket = context.socket(zmq.DEALER)

serverList = []

if socket.connect("tcp://127.0.0.1:5555"):

	queueName = input("Enter your Queue Alias: ")

	message = ""

	while True:
		socket.send_string(queueName)

		time.sleep(1)

		message = socket.recv_multipart()

		if(message[0] not in serverList):
			serverList.append(message[0])
	#sub = {'subscribe':True}



# Do 10 requests, waiting each time for response
#for request in range(10):
#	print(f"Sending request {request}...")
#	socket.send_string("Hello")

	# Get reply
#	message = socket.recv()
#	print(f"Receive reply {request} [ {message} ]")