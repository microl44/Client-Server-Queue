import time
import zmq
import json

context = zmq.Context()
socket = context.socket(zmq.ROUTER)
socket.bind("tcp://127.0.0.1:5555")

clientList = []

print("Server is starting...")

while True:
    # Wait for next request from client
    CID,message = socket.recv_multipart()

    # Do some work
    tempMessage = message.decode("utf-8")

    print(tempMessage)
    print(type(tempMessage))

    tempJson = json.loads(tempMessage)


    print(tempJson)
    print(type(tempJson))

    if("enterQueue" in tempJson.keys()):
    	print(tempJson["enterQueue"])
    	print(tempJson["name"])

    if(CID not in clientList):
        clientList.append(CID)

    message = "This is a message from the server!"
    alteredMessage = bytearray()
    alteredMessage.extend(map(ord, message))

    listToSend = [CID, alteredMessage]
    # Send reply back to client
    socket.send_multipart(listToSend)