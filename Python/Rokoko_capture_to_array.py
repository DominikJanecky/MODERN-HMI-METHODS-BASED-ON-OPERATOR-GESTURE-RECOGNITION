# Import libraries
import socket
import numpy as np
import os
import json
import time

ip_address = '127.0.0.1'    # The localhost
port_number = 14043         # The port used by Rokoko
no_sequences = 60           # Sequences in each class
frames_per_sequence = 90    # Frames in each sequence
actions = np.array(['Starting position','Emergency stop','Movement intensity','Move forward','Move back','Turn left','Turn right','Scaning environment','Activation of robotic arm','Analysing samples','Transmit samples','Start flight'])           # actions to detect
DATA_PATH = os.path.join('C:/SKOLA/DIZERTACKA-Final/SW/Rokoko')           # Path for exported data

# Create folders
for action in actions:
    for sequence in range(no_sequences):
        try:
            os.makedirs(os.path.join(DATA_PATH, action, str(sequence)))
        except:
            pass
        
# create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# bind the socket to a specific IP address and port number
sock.bind((ip_address, port_number))

# print('Get ready')
# time.sleep(5)

# Loop throught actions (gestures)
for action in actions:
    print('Action: ',action)        # Let the user know which gesture is being collected
    time.sleep(5)                   # Time to prepare
    print('BEGIN')                  # Let the user know delay is over
    
    # Loop to collect no_sequences sequences 
    for sequence in range(no_sequences):
        print('Sequence: ',sequence)    # Let the user know number of sequence that is about to be collected
        time.sleep(5)                   # Time to prepare
        
        # Loop to collect frames_per_sequence frames (1 sequence)
        for frame_num in range(frames_per_sequence):
            
            if frame_num % 10 == 0:
                print('Frame ',frame_num)           # every 10th frame number so the user knows how fast to perform gesture
            
            data, address = sock.recvfrom(65000)    # receive data from the socket
            
            data = data.decode("utf-8")             # convert data
            
            # filter data - keep main part of json string
            start_index = data.find('"body":') + len('"body":')     
            end_index = data.find('"face"') - 1
            data = data[start_index:end_index]      
            
            data_dict = json.loads(data)            # load data to dictionary
        
            coordinates = []            # clear list variable
            # extract only numeric values of positions of all keypoints
            for keypoint in data_dict:
                position = data_dict[keypoint]['position']
                coordinates.extend([position['x'], position['y'], position['z']])
            
            # save list to .npy file into corresponding folder
            np.save(os.path.join('C:/SKOLA/DIZERTACKA-Final/SW/Rokoko/%s/%s/' %(action, sequence),str(frame_num)), coordinates[:69])
            
    
print('DONE')   # Let the user know process is done

sock.close()    # End connection with localhost


