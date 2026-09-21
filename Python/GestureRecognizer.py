import os
import socket
import json
import time
import re
import numpy as np
from tensorflow.keras.models import load_model

# Nastavenie debug módu – ak je True, budú sa vypisovať surové dáta
DEBUG = False

# Potlačenie otravného TensorFlow logovania (nepovinné)
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

# Načítanie modelu
model_path = 'C:/SKOLA/DIZERTACKA-Final/SW/Rokoko/weightsNEW3.h5'
model = load_model(model_path)

# Definícia akcií
actions = np.array([
    'Starting position',
    'Emergency stop',
    'Movement intensity',
    'Move forward',
    'Move back',
    'Turn left',
    'Turn right',
    'Scanning environment',
    'Activation of robotic arm',
    'Analysing samples',
    'Transmit samples',
    'Start flight'
])

# Nastavenie UDP socketu na prijímanie dát
ip_address = '127.0.0.1'
port_number = 14043
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((ip_address, port_number))

# Počet rámcov pre jednu sekvenciu
frames_per_sequence = 90
buffer = []

def preprocess_data(body_dict):
    """
    Zo slovníka 'body' vyextrahuje zoznam súradníc.
    """
    coordinates = []
    for keypoint in body_dict:
        position = body_dict[keypoint]['position']
        coordinates.extend([position['x'], position['y'], position['z']])
    return coordinates[:69]

def recognize_gesture(sequence):
    prediction = model.predict(np.expand_dims(sequence, axis=0))[0]
    action_idx = np.argmax(prediction)
    return actions[action_idx]

print("Listening for gesture data...")

try:
    while True:
        data, address = sock.recvfrom(65000)
        data_str = data.decode("utf-8")
        
        if DEBUG:
            print("Prijaté dáta:")
            print(data_str)
        
        # Vyhľadanie všetkých JSON objektov
        json_objects = re.findall(r'(\{.*?\})(?=\s*\{|\s*$)', data_str)
        if not json_objects:
            if DEBUG:
                print("Neboli nájdené žiadne platné JSON objekty.")
            continue

        for obj_str in json_objects:
            try:
                parsed = json.loads(obj_str)
            except json.JSONDecodeError as e:
                if DEBUG:
                    print(f"Chyba pri parsovaní JSON: {e}")
                continue
            
            # Skontrolujeme, či objekt obsahuje "scene" -> "actors"
            if "scene" not in parsed or "actors" not in parsed["scene"]:
                if DEBUG:
                    print("JSON objekt nemá štruktúru 'scene' -> 'actors'.")
                continue

            actors = parsed["scene"]["actors"]
            # Prejdeme cez aktérov; ak aktér obsahuje "body", spracujeme ho
            for actor in actors:
                if "body" in actor:
                    body_data = actor["body"]
                    try:
                        frame = preprocess_data(body_data)
                        buffer.append(frame)
                    except Exception as e:
                        if DEBUG:
                            print(f"Chyba pri spracovaní 'body' dát: {e}")
                        continue

            # Ak je buffer dostatočne dlhý, rozpoznáme gesto
            if len(buffer) >= frames_per_sequence:
                sequence = np.array(buffer[:frames_per_sequence])
                gesture = recognize_gesture(sequence)
                print(f"Recognized gesture: {gesture}")
                # Odstránime spracované rámce
                buffer = buffer[frames_per_sequence:]
                time.sleep(1)

except KeyboardInterrupt:
    print("Terminated by user")

finally:
    sock.close()
    print("Socket closed")
