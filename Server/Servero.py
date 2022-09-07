import time
import zmq

context = zmq.Context()
socket = context.socket(zmq.ROUTER)
socket.bind("tcp://*:5555")

clientList = []

print("Server is starting...")

while True:
    # Wait for next request from client
    CID,message = socket.recv_multipart()

    # Do some work
    print(message)

    if(CID not in clientList):
    	clientList.append(CID)

    message = "This is a message from the server!"
    alteredMessage = bytearray()
    alteredMessage.extend(map(ord, message))

    listToSend = [alteredMessage]
    # Send reply back to client
    socket.send_multipart(listToSend)