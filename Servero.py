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
            
            #Send message back to client
            msg = [clientID, messageRes]
            socket.send_multipart(msg)
            
    # If client want status update
    if 'subscribe' in messageDict:
        # Check if client want to subscribe
        if messageDict['subscribe']:
            # Check if client is already subscribed
            if clientID not in subsQueue:
                subsQueue.append(clientID)
                print('Client successfully subscribed...')
            
            else:
                print('Client is already subscribed...')
                
            # Prepare and send message to client
            statusDict = {'queue' : queueTickets}
            messageRes = json.dumps(statusDict)
            messageRes = str.encode(messageRes)
            msg = [clientID, messageRes]
            print(msg)
            socket.send_multipart(msg)
        
        # Unsubscribe client
        else:
            # Remove client from list as long as list is not empty
            if subsQueue:
                # If client is subscribed
                if clientID in subsQueue:
                    subsQueue.pop(subsQueue.index(clientID))
                    print('Client has been unsubscribed...')
                
                else:
                    print('Client not found in queue...')
            
            else:
                print('No clients are subscribed...')
    
    # If queue has been updated            
    if updateStatus:
        # For every client in the subscribed list
        for client in subsQueue:
            # Prepare and send message to client
            statusDict = {'queue' : queueTickets}
            messageRes = json.dumps(statusDict)
            messageRes = str.encode(messageRes)
            msg = [clientID, messageRes]
            socket.send_multipart(msg)
        # Reset status
        updateStatus = False
        
    else:
        messageRes = {}
        messageRes = json.dumps(messageRes)
        messageRes = str.encode(messageRes)
        temp = [clientID, messageRes]
    # Send reply back to client
        socket.send_multipart(temp)
    # Do some work
    time.sleep(1)
