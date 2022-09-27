from asyncio import format_helpers
from http import client
import zmq
import time
import json
import threading

subsQueue = []
studentQueue = []
supervisorIDs = []
supervisorList  = []
ticketQueue = []
clientIDsList = []
heartbeatList = []
serverId = '1'
t = 15
port = 5555

def protocol_Heartbeat():
    # Global
    global heartbeatList, clientIDsList, supervisorIDs, serverId, updateStatus, socket, t
    # Unstoppable loop
    while True:
        # Iterate through all clients and send a heartbeat message
        for c in clientIDsList:
            # Construct message
            heartbeat = {'serverId': serverId}
            heartbeat_message = json.dumps(heartbeat)
            # Encode message
            heartbeat_message = str.encode(heartbeat_message)
            # Prepare final message
            heartbeat_msg = [c, heartbeat_message]
            # Send message
            socket.send_multipart(heartbeat_msg)
    
        time.sleep(t)
        # Iterate through all heartbeat responses and trim all lists with clients
        aliveClients = []
        deadClients = []
        for c in clientIDsList:
            # Client responded to heartbeat or other message
            if c in heartbeatList:
                aliveClients.append(c)
            # No response from client at all
            elif c not in heartbeatList:
                deadClients.append(c)
        # Check remaining list with clients due to timeout
        for dead in deadClients:
            # Dead students
            for student in studentQueue:
                if dead in student:
                    # Update all clients
                    updateStatus = True
                    ticketQueue.pop(studentQueue.index(student))
                    for ticket in ticketQueue:
                        ticket['ticket'] = ticketQueue.index(ticket) + 1
                    studentQueue.remove(student)
                    print('\tServer: Timeout for student client...\n')
            # Dead subscribers
            for sub in subsQueue:
                if dead in subsQueue:
                    # Update all clients
                    updateStatus = True
                    subsQueue.remove(sub)
                    print('\tServer: Timeout for subscribed client...\n')
            # Dead supervisors
            for supervisor in supervisorIDs:
                if dead in supervisor:
                    # Find correct name on supervisor with dead ID
                    for super in supervisorList:
                        if supervisor[1] == super['name']:
                            updateStatus = True
                            supervisorList.remove(super)
                            supervisorIDs.remove(supervisor)
                            print('\tServer: Timeout for supervisor client...\n')

        clientIDsList = aliveClients
        heartbeatList = []

def protocol_Attend(client_id, message_dict):
    # Global
    global updateStatus, supervisorList
    # Check if supervisor client wants to join class
    if message_dict['attend']:
        # If first supervisor to join
        if not supervisorList:
            updateStatus = True
            # Add supervisor client to list
            supervisorList.append({'name':message_dict['name'], 'status':'available', 'client':'', 'clientMessage':''}) 
            supervisorIDs.append([client_id, message_dict['name']])
            print('\tServer: First supervisor added to class...\n')
        else:
            match_name = False
            # Check if supervisor client is already in class with same name
            for supervisor in supervisorList:
                if message_dict['name'] == supervisor['name']:
                    updateStatus = False
                    match_name = True
                    print('\tServer: A supervisor with that name is already attending class...\n')
                    break
            if not match_name:
                updateStatus = True
                # Add supervisor client to list
                supervisorList.append({'name':message_dict['name'], 'status':'available', 'client':'', 'clientMessage':''})
                supervisorIDs.append([client_id, message_dict['name']])
                print('\tServer: Supervisor is added to class...\n')  
         
    elif not message_dict['attend']:
        # If no supervisors
        if not supervisorList:
            print('\tServer: No supervisor attending class...\n')
        else:
            # Remove supervisor ID
            for superID in supervisorIDs:
                if client_id in superID and superID[1] == message_dict['name']:
                    updateStatus = True
                    supervisorIDs.remove(superID)
            # Find supervisor in list and remove
            for supervisor in supervisorList:
                if message_dict['name'] == supervisor['name']:
                    updateStatus = True
                    # Remove supervisor
                    supervisorList.remove(supervisor)
                    print('\tServer: A supervisor has left class...\n')
                        
def protocol_EnterQueue(client_id, message_dict):
    # Global
    global updateStatus, studentQueue, ticketQueue, socket, serverId
    # Check if student client wants to enter queue
    if message_dict['enterQueue']:
        # If first student client to join
        if not studentQueue:
            updateStatus = True
            # Add first student
            studentQueue.append([client_id, message_dict['name']])     
            # Construct message for response
            ticketDict = {'ticket': 1, 'name': message_dict['name']}
            message_ticket = {'ticket': 1, 'name': message_dict['name'], 'serverId': serverId}
            ticketQueue.append(ticketDict)
            message_response = json.dumps(message_ticket)
            # Encode message
            message_response = str.encode(message_response)
            # Send response to student
            msg = [client_id, message_response]
            socket.send_multipart(msg)
            print('\tServer: Student is first to enter queue...\n')
        else:
            match_id = False
            match_name = False
            for student in studentQueue:
                # Check if student client is already in queue with same name
                if client_id in student:
                    match_id = True
                    if message_dict['name'] in student:
                        updateStatus = False
                        match_name = True
                        # Get the student's ticket and resend if name matches
                        student_ticket = ticketQueue[studentQueue.index(student)]
                        if student_ticket['name'] in student:
                            student_ticket['serverId'] = serverId
                            # Construct message for response
                            message_response = json.dumps(student_ticket)
                            # Encode message
                            message_response = str.encode(message_response)
                            # Send response to student
                            msg = [client_id, message_response]
                            socket.send_multipart(msg)
                        print('\tServer: The student is already in the queue...\n')
                        break
            if not (match_id and match_name):
                updateStatus = True
                # Add student client to list
                studentQueue.append([client_id, message_dict['name']])
                # Construct ticket for student
                ticket = {'ticket': len(studentQueue), 'name': message_dict['name']}
                message_ticket = {'ticket': len(studentQueue), 'name': message_dict['name'], 'serverId': serverId}
                ticketQueue.append(ticket)
                # Construct message for response 
                message_response = json.dumps(message_ticket)
                # Encode message
                message_response = str.encode(message_response)
                # Send response to student
                msg = [client_id, message_response]
                socket.send_multipart(msg)
                print('\tServer: New student added to queue... New ticket created...\n')
    
def protocol_Subscribe(client_id, message_dict):
    # Global
    global ticketQueue, subsQueue
    # Check if client want to subscribe to updates
    if message_dict['subscribe']:
        # Check if client is subscribed
        if client_id not in subsQueue:
            subsQueue.append(client_id)
            print('\t\tServer: Client successfully subscribed...\n')
        else:
            print('\t\tServer: Client is already subscribed...\n')
            
        # Construct message for response
        status_dict = {'queue' : ticketQueue, 'supervisors': supervisorList, 'serverId': serverId}
        message_response = json.dumps(status_dict)
        # Encode message
        message_response = str.encode(message_response)
        msg = [client_id, message_response]
        # Send response to client
        socket.send_multipart(msg)

    elif not message_dict['subscribe']:
        # Remove client from list as long as list is not empty
        if subsQueue:
            # If client is subscribed
            if client_id in subsQueue:
                subsQueue.pop(subsQueue.index(client_id))
                print('\t\tServer: Client has been unsubscribed...\n')
            else:
                print('\t\tServer: Client not found in queue...\n')
    print('\t\tServer: Subscriptions has been updated, new total is ' + str(len(subsQueue)) + '\n')

def protocol_Remove(message_dict):
    # Global
    global ticketQueue, studentQueue, updateStatus, serverId
    # Check if goal is to remove student
    if message_dict['remove']:
        # Check if queue is empty
        if not studentQueue:
            ('\t\t\tServer: Failed to remove from queue... Queue is already empty...')
        elif studentQueue: 
            # Check if only one client in queue
            if len(studentQueue) == 1:
                # Remove client from queue
                message_student = studentQueue.pop(0)
                student_ticket = ticketQueue.pop(0)
                updateStatus = True
                # Find supervisor
                for super in supervisorList:
                    if message_dict['name'] in super['name']:
                        # Update values for the supervisor
                        super['status'] = 'occupied'
                        super['client'] = student_ticket
                        super['clientMessage'] = message_dict['message']
                        # Construct message for response 
                        student_message = {'name': message_dict['name'], 'clientMessage': message_dict['message'], 'serverId': serverId}
                        message_response = json.dumps(student_message)
                        # Encode message
                        message_response = str.encode(message_response)
                        # Send response to student
                        msg = [message_student[0], message_response]
                        socket.send_multipart(msg)
                        break
            # Check if more than one client in queue
            elif len(studentQueue) > 1:
                # Remove client from queue
                message_student = studentQueue.pop(0)
                student_ticket = ticketQueue.pop(0)
                updateStatus = True
                # Find supervisor
                for super in supervisorList:
                    if message_dict['name'] in super['name']:
                        # Update values for the supervisor
                        super['status'] = 'occupied'
                        super['client'] = student_ticket
                        super['clientMessage'] = message_dict['message']
                        # Construct message for response 
                        student_message = {'name': message_dict['name'], 'clientMessage': message_dict['message'], 'serverId': serverId}
                        message_response = json.dumps(student_message)
                        # Encode message
                        message_response = str.encode(message_response)
                        # Send response to student
                        msg = [message_student[0], message_response]
                        socket.send_multipart(msg)
                        # Update remaining tickets in queue
                        for ticket in ticketQueue:
                            ticket['ticket'] = ticketQueue.index(ticket) + 1
                        break
        print('\t\t\tServer: Changes were made to student queue...')
    
    elif not message_dict['remove']:
        # Check if queue is empty
        if not studentQueue:
            ('\t\t\tServer: Failed to send message... Queue is empty...')
        elif studentQueue: 
            # Check if only one client in queue
            if len(studentQueue) == 1:
                # Find supervisor
                for super in supervisorList:
                    if message_dict['name'] in super['name']:
                        # Update values for the supervisor
                        super['status'] = 'occupied'
                        super['client'] = ''
                        super['clientMessage'] = ''
                        # Construct message for response 
                        student_message = {'name': message_dict['name'], 'clientMessage': message_dict['message'], 'serverId': serverId}
                        message_response = json.dumps(student_message)
                        # Encode message
                        message_response = str.encode(message_response)
                        # Send response to student
                        msg = [studentQueue[0][0], message_response]
                        socket.send_multipart(msg)
                        break
            # Check if more than one client in queue
            elif len(studentQueue) > 1:
                # Find supervisor
                for super in supervisorList:
                    if message_dict['name'] in super['name']:
                        # Update values for the supervisor
                        super['name'] = 'occupied'
                        super['client'] = ''
                        super['clientMessage'] = ''
                        # Construct message for response 
                        student_message = {'name': message_dict['name'], 'clientMessage': message_dict['message'], 'serverId': serverId}
                        message_response = json.dumps(student_message)
                        # Encode message
                        message_response = str.encode(message_response)
                        # Send response to student  
                        msg = [studentQueue[0][0], message_response]
                        socket.send_multipart(msg)
                        break
        print('\t\t\tServer: Message sent to student...')

print('Server is starting...\n')
print('Please input an ID for the server: ')
serverId = str(input())
print('\nContinue with selecting time in seconds for client timeout: ')
t = int(input())
print('\nAt last, select a port number for the server: ')
port = int(input())

context = zmq.Context()
socket = context.socket(zmq.ROUTER)
socket.bind(f'tcp://*:{port}')

print('Configuring server... Please wait...')

heartThread = threading.Thread(target=protocol_Heartbeat)
heartThread.start()

print('Server is up and running...')

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
    
    if 'attend' in messageDict:
        if clientID not in heartbeatList:
            heartbeatList.append(clientID)   
        protocol_Attend(clientID, messageDict)
        
    if 'enterQueue' in messageDict:
        if clientID not in heartbeatList:
            heartbeatList.append(clientID)   
        protocol_EnterQueue(clientID, messageDict)

    if 'subscribe' in messageDict:
        if clientID not in heartbeatList:
            heartbeatList.append(clientID)   
        protocol_Subscribe(clientID, messageDict)

    if 'remove' in messageDict:
        if clientID not in heartbeatList:
            heartbeatList.append(clientID)   
        protocol_Remove(messageDict)
        
    if not messageDict:
        if clientID not in heartbeatList:
            heartbeatList.append(clientID) 

    # If queue has been updated            
    if updateStatus:
        # Prepare general message
        status_message = {'queue' : ticketQueue, 'supervisors': supervisorList, 'serverId': serverId}
        # Construct message
        message_response = json.dumps(status_message)
        # Encode message
        message_response = str.encode(message_response)
        # Send status update to subscribed clients
        for c in subsQueue:
            msg = [c, message_response]
            socket.send_multipart(msg)
        # Reset status
        updateStatus = False
