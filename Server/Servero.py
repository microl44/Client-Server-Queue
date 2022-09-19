from asyncio import format_helpers
from http import client
import zmq
import time
import json
import threading

print('Server is starting...')

context = zmq.Context()
socket = context.socket(zmq.ROUTER)
socket.bind('tcp://*:5555')

print('Configuring server... Please wait...')

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
    print(f'\nReceived request: {message} from {clientID}...\n')
    
    # First, check if message from new client
    if clientID not in clientIDsList:
        # If not present, add client to list
        clientIDsList.append(clientID)

    # Process request, convert from json to dict
    messageDict = json.loads(message)
    
    if 'enterQueue' in messageDict:
        # If want to join queue
        if messageDict['enterQueue']:
            # Set to send status to all
            updateStatus = True
            # Should update status for everybody
            updateStatus = True
            # Check if new client wants to join queue
            if not clientQueue:
                # Add first to queue
                clientQueue.append([clientID, messageDict['name']])     
                # Create a response to the client for request
                ticketDict = {'ticket': 0, 'name': messageDict['name']}
                queueTickets.append(ticketDict)
                messageRes = json.dumps(ticketDict)
                # Encode message
                messageRes = str.encode(messageRes)
                
                #Send message back to client
                msg = [clientID, messageRes]
                socket.send_multipart(msg)
                print('\tServer: Client is first to enter queue... New ticket...\n')
            else:
                # Value to determine if match is found
                clientMatch = False
                for pair in clientQueue:
                    # Check if requesting client is already in queue
                    if clientID in pair:
                        # Match found
                        clientMatch = True
                        clientNameMatch = False
                        # Check if new clients uses old alias
                        for ticket in queueTickets:
                            if messageDict['name'] == ticket['name']:
                                # Match found
                                clientNameMatch = True
                                #Prepare message
                                messageRes = json.dumps(ticket)
                                messageRes = str.encode(messageRes)
                                
                                #Send message back to client
                                msg = [clientID, messageRes]
                                socket.send_multipart(msg)
                                print('\tServer: Client with that name is already in queue, returned same ticket...\n')
                                break
                        # Old client uses new alias
                        if not clientNameMatch:    
                            # Add old client with new name to queue
                            clientQueue.append([clientID, messageDict['name']])     
                            # Create a response to the client for request
                            ticketDict = {'ticket': len(clientQueue) - 1, 'name': messageDict['name']}
                            queueTickets.append(ticketDict)
                            messageRes = json.dumps(ticketDict)
                            messageRes = str.encode(messageRes)
                            
                            #Send message back to client
                            msg = [clientID, messageRes]
                            socket.send_multipart(msg)
                            print('\tServer: Old client and new alias identified... New ticket...\n')
                        break
                # New client
                if not clientMatch:
                    # Match found
                    clientNameMatch = False
                    # Check if new client uses old alias
                    for ticket in queueTickets:
                        if messageDict['name'] == ticket['name']:
                            # Match found
                            clientNameMatch = True
                            # Add old client with new name to queue
                            clientQueue.append([clientID, messageDict['name']])     
                            # Create a response to the client for request
                            ticketDict = {'ticket': len(clientQueue) - 1, 'name': messageDict['name']}
                            queueTickets.append(ticketDict)
                            #Prepare message
                            messageRes = json.dumps(ticketDict)
                            messageRes = str.encode(messageRes)
                            
                            #Send message back to client
                            msg = [clientID, messageRes]
                            socket.send_multipart(msg)
                            print('\tServer: New client, old name identified... New ticket...\n')
                            break
                    # New client uses new alias
                    if not clientNameMatch:
                        #Add new client with new name to queue
                        clientQueue.append([clientID, messageDict['name']])     
                        # Create a response to the client for request
                        ticketDict = {'ticket': len(clientQueue) - 1, 'name': messageDict['name']}
                        queueTickets.append(ticketDict)
                        messageRes = json.dumps(ticketDict)
                        messageRes = str.encode(messageRes)
                        
                        #Send message back to client
                        msg = [clientID, messageRes]
                        socket.send_multipart(msg)
                        print('\tServer: New client and new alias identified... New ticket...\n')        

    # If client want status update
    if 'subscribe' in messageDict:
        # Check if client want to subscribe
        if messageDict['subscribe']:
            # Check if client is already subscribed
            if clientID not in subsQueue:
                subsQueue.append(clientID)
                print('\t\tServer: Client successfully subscribed...\n')
            
            else:
                print('\t\tServer: Client is already subscribed...\n')
                
            # Prepare and send message to client
            statusDict = {'queue' : queueTickets}
            messageRes = json.dumps(statusDict)
            messageRes = str.encode(messageRes)
            msg = [clientID, messageRes]
            socket.send_multipart(msg)
        
        # Unsubscribe client
        else:
            # Remove client from list as long as list is not empty
            if subsQueue:
                # If client is subscribed
                if clientID in subsQueue:
                    subsQueue.pop(subsQueue.index(clientID))
                    print('\t\tServer: Client has been unsubscribed...\n')
                
                else:
                    print('\t\tServer: Client not found in queue...\n')

        print('\t\tServer: Subscriptions has been updated, new total is' + len(subsQueue) + '\n')
    
    # If remove request is received
    if 'remove' in messageDict:
        # Check if queue is empty
        if not clientQueue:
            ('\t\t\tServer: Failed to remove from queue... Queue is already empty...')
        elif clientQueue: 
            # Check if only one client in queue
            if len(clientQueue) == 0:
                # Remove client from queue
                clientQueue.pop(0)
                queueTickets.pop(0)
                updateStatus = True
            # Check if more than one client in queue
            elif len(clientQueue) > 0:
                # Remove client from queue
                clientQueue.pop(0)
                queueTickets.pop(0)
                updateStatus = True
                # Update remaining clients in queue
                for ticket in queueTickets:
                    ticket['ticket'] = queueTickets.index(ticket)
        # Status update to log
        print('\t\t\tServer: Changes were made to queue...')
                    
    # If queue has been updated            
    if updateStatus:
        # For every client in the subscribed list
        for c in subsQueue:
            # Prepare and send message to client
            statusDict = {'queue' : queueTickets}
            messageRes = json.dumps(statusDict)
            messageRes = str.encode(messageRes)
            msg = [clientID, messageRes]
            socket.send_multipart(msg)
        # Reset status
        updateStatus = False
        
    if '' == messageDict:
        messageRes = {}
        messageRes = json.dumps(messageRes)
        messageRes = str.encode(messageRes)
        temp = [clientID, messageRes]
        # Send reply back to client
        socket.send_multipart(temp)
    # Do some work
    time.sleep(1)
