# Import libraries
import numpy as np
import os
from sklearn.model_selection import train_test_split
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import LSTM, Dense, Dropout
from tensorflow.keras.regularizers import l2
from tensorflow.keras.callbacks import TensorBoard, ReduceLROnPlateau, EarlyStopping

# Data preparation
no_sequences = 80           # Sequences in each class
frames_per_sequence = 90    # Frames in each sequence
actions = np.array(['Starting position','Emergency stop','Movement intensity','Move forward','Move back','Turn left','Turn right','Scaning environment','Activation of robotic arm','Analysing samples','Transmit samples','Start flight'])           # actions to detect
DATA_PATH = os.path.join('C:/SKOLA/DIZERTACKA-Final/SW/Rokoko')  # Path for exported data

label_map = {label: num for num, label in enumerate(actions)}  # Create dictionary of actions

sequences, labels = [], []
for action in actions:
    for sequence in range(no_sequences):
        window = []
        for frame_num in range(frames_per_sequence):
            res = np.load(os.path.join(DATA_PATH, action, str(sequence), f"{frame_num}.npy"))
            window.append(res)
        sequences.append(window)
        labels.append(label_map[action])

X = np.array(sequences)
y = to_categorical(labels).astype(int)

# Stratified sampling for training and validation data
#X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)

# Manually split data into training and test sets
train_indices, test_indices = [], []
for i in range(len(actions)):
    start_index = i * no_sequences
    end_index = start_index + no_sequences
    train_indices.extend(range(start_index, start_index + int(0.8 * no_sequences)))
    test_indices.extend(range(start_index + int(0.8 * no_sequences), end_index))

X_train, X_test = X[train_indices], X[test_indices]
y_train, y_test = y[train_indices], y[test_indices]



# Neural network architecture with L2 regularization
model = Sequential()
model.add(LSTM(128, return_sequences=True, input_shape=(90, 69), kernel_regularizer=l2(0.01)))
model.add(Dropout(0.5))  # Zvýšený dropout rate
model.add(LSTM(64, kernel_regularizer=l2(0.01)))
model.add(Dropout(0.5))  # Zvýšený dropout rate
model.add(Dense(32, activation='relu', kernel_regularizer=l2(0.01)))
model.add(Dense(len(actions), activation='softmax', kernel_regularizer=l2(0.01)))
model.compile(optimizer='Adam', loss='categorical_crossentropy', metrics=['accuracy'])

# Setup TensorBoard, ReduceLROnPlateau and EarlyStopping
log_dir = os.path.join('C:/SKOLA/DIZERTACKA-Final/SW/Rokoko/Logs')
tb_callback = TensorBoard(log_dir=log_dir)
reduce_lr = ReduceLROnPlateau(monitor='val_loss', factor=0.2, patience=5, min_lr=0.00001)
early_stopping = EarlyStopping(monitor='val_loss', patience=10, restore_best_weights=True)

# Train the model
model.fit(X_train, y_train, epochs=50, batch_size=32, validation_data=(X_test, y_test), callbacks=[tb_callback, reduce_lr, early_stopping])

# Save the model
model.save('C:/SKOLA/DIZERTACKA-Final/SW/Rokoko/weightsNEW3.h5')

# Optionally: Print the model summary
model.summary()
