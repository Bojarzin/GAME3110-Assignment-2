import socket
import time
import _thread
import threading
import json

from datetime import datetime
from enum import IntEnum

class SocketMessageTypes(IntEnum):
    NONE = 0
    CONNECT = 1        # Sent from client to server
    CHOICE = 2         # Sent from client to server
    GAME_RULES = 3     # Sent from server to client
    RESULT = 4         # Sent from server to client

clients = {}

def send_game_rules_to_client(sock: socket.socket, addr):
    payload = {}
    payload['header'] = SocketMessageTypes.GAME_RULES
    payload['message'] = "Hello"

    payload = json.dumps(payload).encode('utf-8')
    sock.sendto(bytes(payload), addr)

def handle_messages(sock: socket.socket):
    print('Listening to messages on new thread')
    receivedChoices = 0
    choiceOne = ""
    choiceTwo = ""
    playerID = 1
    attempts = 0
    while True:
        data, addr = sock.recvfrom(1024)
        data = str(data.decode('utf-8'))
        data = json.loads(data)

        #print(f'Received message from {addr}:{data}')
        #print(receivedChoices)

        if addr in clients: 
            if (data['header'] == SocketMessageTypes.CHOICE):
                if (choiceOne == ""):
                    if (clients[addr]['choice'] == ""):
                        receivedChoices = receivedChoices + 1
                        clients[addr]['choice'] = data['choice']
                        choiceOne = clients[addr]['id'] + " chose " + clients[addr]['choice']
                        clients[addr]['choice'] = ""
                elif (choiceTwo == ""):
                    attempts = attempts + 1
                    if (clients[addr]['choice'] == ""):
                        receivedChoices = receivedChoices + 1
                        clients[addr]['choice'] = data['choice']
                        choiceTwo =  clients[addr]['id'] + " chose " + clients[addr]['choice']
                        clients[addr]['choice'] = ""

                if (receivedChoices >= 2):
                    payload = {}
                    payload['header'] = SocketMessageTypes.RESULT
                    payload['message'] = choiceOne + " and " + choiceTwo
                    payload = json.dumps(payload).encode('utf-8')
                    for c in clients:
                        sock.sendto(bytes(payload), (c[0], c[1]))
                    receivedChoices = 0
                    print(choiceOne + " One " + choiceTwo + " Two ")
                    choiceOne = ""
                    choiceTwo = ""
                    print(choiceOne + " One " + choiceTwo + " Two ")

                print(clients[addr]['choice'])
            elif (data['heartBeat'] == "heartbeat"):
                clients[addr]['last heartbeat'] = datetime.now()
        else:
            if (data['header'] == SocketMessageTypes.CONNECT):
                clients[addr] = {}
                clients[addr]['choice'] = ""
                clients[addr]['last heartbeat'] = datetime.now()
                clients[addr]['id'] = str(playerID)
                payload = {}
                payload['header'] = SocketMessageTypes.CONNECT
                payload['message'] = str(addr) + " connected"
                payload['id'] = clients[addr]['id']
                payload = json.dumps(payload).encode('utf-8')
                sock.sendto(bytes(payload), addr)
                send_game_rules_to_client(sock, addr)
                playerID = playerID + 1

def clearDroppedClients():
    while True:
        print (clients)
        for c in list(clients.keys()):
            if (datetime.now() - clients[c]['last heartbeat']).total_seconds() > 5:
                print('Client dropped: ', c)
                del clients[c]
        time.sleep(1)


def main():
    print("Starting server")
    port = 12345
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.bind(('', port))

    _thread.start_new_thread(handle_messages, (s,))
    _thread.start_new_thread(clearDroppedClients, ())
    ##_thread.start_new_thread(gameLoop, (s,))

    while True:
        time.sleep(1)

if __name__ == '__main__':
    main()