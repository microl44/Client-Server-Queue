from http import client
import zmq
import time
import json

context = zmq.Context()
socket = context.socket(zmq.ROUTER)
socket.bind('tcp://*:5555')

print('Server is starting...')

# Create a queue to hold the waiting clients
queueTickets = []
clientQueue = []
subsQueue = []
clientIDsList = []

while True:
    # Variable to tell if need exist to update all subscribed clients, false at start
    updateStatus = False
    
    # If message, get the next request from client
    clientID, message = socket.recv_multipart()
    print(f'Received request: {message} from {clientID}...')
    
    # First, check if message from new client
    if clientID not in clientIDsList:
        # If not present, add client to list
        clientIDsList.append(clientID)

    # Process request, convert from json to dict
    messageDict = json.loads(message)
    
    if 'enterQueue' in messageDict:
        # If want to join queue
        if messageDict['enterQueue']:
            # Should update status for everybody
            updateStatus = True
            # Check if already in queue
            if clientID not in queueTickets:
                clientQueue.append(clientID)
            
            # Create a response to the client for request
            ticketDict = {'ticket': clientQueue.index(clientID), 'name': messageDict['name']}
            queueTickets.append(ticketDict)
            messageRes = json.dumps(ticketDict)
            # Encode message
            messageRes = str.encode(messageRes)
            
            #Send message
            msg = [clientID]

    if updateStatus:
        for client in clientIDsList:
            statusDict = {'queue':queueTickets}
            statusDict = json.dumps(statusDict)
            statusDict = str.encode(statusDict)
            temo = [client, statusDict]
            print(temo)
            socket.send_multipart(temo)
        updateStatus = False
    else:
        temp = [clientID, messageRes]
    # Send reply back to client
        socket.send_multipart(temp)
    # Do some work
    time.sleep(1)
