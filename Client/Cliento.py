import zmq

context = zmq.Context()
# Socket to talk to server
print("Connecting to server...")
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:5555")

# Do 10 requests, waiting each time for response
for request in range(10):
    print(f"Sending request {request}...")
    socket.send_string("Hello")

    # Get reply
    message = socket.recv()
    print(f"Receive reply {request} [ {message} ]")
