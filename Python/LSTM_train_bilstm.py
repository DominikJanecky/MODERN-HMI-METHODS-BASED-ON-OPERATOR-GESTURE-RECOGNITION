"""Train a BiLSTM variant based on the article's sixth model architecture.

The model consumes 90 frames with 69 motion-capture features per frame and
classifies them into one of twelve gesture classes. The article does not publish
the trained weights or every hyperparameter, so this script cannot by itself
reproduce the reported 95.96% validation accuracy.
"""

import os

import numpy as np
from sklearn.model_selection import train_test_split
from tensorflow.keras.callbacks import ReduceLROnPlateau, TensorBoard
from tensorflow.keras.layers import BatchNormalization, Bidirectional, Dense, Dropout, Input, LSTM
from tensorflow.keras.models import Sequential
from tensorflow.keras.regularizers import l2
from tensorflow.keras.utils import to_categorical


# Update these paths before running the script on another machine.
DATA_PATH = r"C:/SKOLA/DIZERTACKA-Final/SW/Rokoko"
MODEL_PATH = os.path.join(DATA_PATH, "weights_bilstm_reconstruction.h5")
LOG_DIR = os.path.join(DATA_PATH, "Logs", "bilstm_reconstruction")

# The article specifies 60 original recordings per class but not the final
# augmented count. 80 is carried over from the earlier repository script.
NUM_SEQUENCES = 80
FRAMES_PER_SEQUENCE = 90
FEATURES_PER_FRAME = 69
EPOCHS = 50
BATCH_SIZE = 32
VALIDATION_FRACTION = 0.2
RANDOM_SEED = 42

ACTIONS = np.array([
    "Starting position",
    "Emergency stop",
    "Movement intensity",
    "Move forward",
    "Move back",
    "Turn left",
    "Turn right",
    "Scanning environment",
    "Activation of robotic arm",
    "Analysing samples",
    "Transmit samples",
    "Start flight",
])

# The recording script spells this directory name "Scaning environment".
# Keep the published class label while loading the folders it actually wrote.
DATA_DIR_NAMES = {
    "Scanning environment": "Scaning environment",
}


def load_dataset():
    """Load the recorded NumPy sequences and their gesture labels."""
    label_map = {label: index for index, label in enumerate(ACTIONS)}
    sequences, labels = [], []

    for action in ACTIONS:
        action_dir = DATA_DIR_NAMES.get(action, action)
        for sequence in range(NUM_SEQUENCES):
            frames = []
            for frame_number in range(FRAMES_PER_SEQUENCE):
                path = os.path.join(DATA_PATH, action_dir, str(sequence), f"{frame_number}.npy")
                frames.append(np.load(path))
            sequences.append(frames)
            labels.append(label_map[action])

    features = np.asarray(sequences, dtype=np.float32)
    expected_shape = (len(ACTIONS) * NUM_SEQUENCES, FRAMES_PER_SEQUENCE, FEATURES_PER_FRAME)
    if features.shape != expected_shape:
        raise ValueError(f"Expected dataset shape {expected_shape}, got {features.shape}")
    targets = to_categorical(labels, num_classes=len(ACTIONS)).astype(np.float32)
    return features, targets, np.asarray(labels)


def split_dataset(features, targets, labels):
    """Make the 80/20 stratified split described in the article."""
    return train_test_split(
        features,
        targets,
        test_size=VALIDATION_FRACTION,
        random_state=RANDOM_SEED,
        stratify=labels,
    )


def build_model():
    """Build the Training 6 architecture shown in Figure 9 of the article."""
    # The L2 coefficient and 32-unit dense layer are inherited from the
    # earlier script; the article does not state their exact values.
    regularizer = l2(0.01)
    model = Sequential([
        Input(shape=(FRAMES_PER_SEQUENCE, FEATURES_PER_FRAME)),
        Bidirectional(
            LSTM(
                128,
                return_sequences=True,
                kernel_regularizer=regularizer,
            ),
        ),
        Dropout(0.5),
        BatchNormalization(),
        Bidirectional(LSTM(64, kernel_regularizer=regularizer)),
        Dropout(0.5),
        BatchNormalization(),
        Dense(32, activation="relu", kernel_regularizer=regularizer),
        Dense(len(ACTIONS), activation="softmax", kernel_regularizer=regularizer),
    ])
    model.compile(
        optimizer="Adam",
        loss="categorical_crossentropy",
        metrics=["accuracy"],
    )
    return model


def main():
    features, targets, labels = load_dataset()
    x_train, x_validation, y_train, y_validation = split_dataset(features, targets, labels)

    model = build_model()
    callbacks = [
        TensorBoard(log_dir=LOG_DIR),
        ReduceLROnPlateau(monitor="val_loss", factor=0.2, patience=5, min_lr=1e-5),
    ]
    model.fit(
        x_train,
        y_train,
        epochs=EPOCHS,
        batch_size=BATCH_SIZE,
        validation_data=(x_validation, y_validation),
        callbacks=callbacks,
    )
    model.save(MODEL_PATH)
    model.summary()


if __name__ == "__main__":
    main()
