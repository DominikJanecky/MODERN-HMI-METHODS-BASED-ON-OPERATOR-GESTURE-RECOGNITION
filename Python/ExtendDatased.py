# import os
# import numpy as np
# import random
# from pathlib import Path

# def add_noise(data, noise_level=0.01):
#     noise = np.random.normal(0, noise_level, data.shape)
#     return data + noise

# def process_gesture_folder(gesture_folder_path, output_base_path, num_subfolders_to_process=30, noise_level=0.01):
#     # Get all subfolders
#     subfolders = [f for f in os.listdir(gesture_folder_path) if os.path.isdir(os.path.join(gesture_folder_path, f))]
    
#     # Randomly select 30 subfolders
#     selected_subfolders = random.sample(subfolders, num_subfolders_to_process)
    
#     for subfolder in selected_subfolders:
#         subfolder_path = os.path.join(gesture_folder_path, subfolder)
#         npy_files = [f for f in os.listdir(subfolder_path) if f.endswith('.npy')]
        
#         # Create output subfolder
#         output_subfolder_path = os.path.join(output_base_path, f"{subfolder} - EDITED")
#         os.makedirs(output_subfolder_path, exist_ok=True)
        
#         for npy_file in npy_files:
#             npy_file_path = os.path.join(subfolder_path, npy_file)
            
#             # Load the .npy file
#             data = np.load(npy_file_path)
            
#             # Add noise to the data
#             augmented_data = add_noise(data, noise_level)
            
#             # Save the augmented data to the new output folder
#             output_npy_file_path = os.path.join(output_subfolder_path, npy_file)
#             np.save(output_npy_file_path, augmented_data)

# def main():
#     base_folder_path = r'C:\SKOLA\DIZERTACKA-Final\SW\Rokoko'
#     output_base_path = r'C:\SKOLA\DIZERTACKA-Final\SW\Rokoko2'

#     gesture_folders = [f for f in os.listdir(base_folder_path) if os.path.isdir(os.path.join(base_folder_path, f))]

#     for gesture_folder in gesture_folders:
#         gesture_folder_path = os.path.join(base_folder_path, gesture_folder)
#         process_gesture_folder(gesture_folder_path, os.path.join(output_base_path, gesture_folder))
        
# if __name__ == "__main__":
#     main()



import os
import numpy as np
import random
from pathlib import Path

def add_noise(data, noise_level=0.01):
    noise = np.random.normal(0, noise_level, data.shape)
    return data + noise

def process_gesture_folder(gesture_folder_path, output_base_path, num_subfolders_to_process=30, noise_level=0.01):
    # Get all subfolders
    subfolders = [f for f in os.listdir(gesture_folder_path) if os.path.isdir(os.path.join(gesture_folder_path, f))]
    
    # If there are fewer subfolders than the number to process, use all subfolders
    if len(subfolders) < num_subfolders_to_process:
        selected_subfolders = subfolders
    else:
        selected_subfolders = random.sample(subfolders, num_subfolders_to_process)
    
    for idx, subfolder in enumerate(selected_subfolders):
        subfolder_path = os.path.join(gesture_folder_path, subfolder)
        npy_files = [f for f in os.listdir(subfolder_path) if f.endswith('.npy')]
        
        # Create output subfolder with a new number
        new_folder_number = 60 + idx
        output_subfolder_path = os.path.join(output_base_path, str(new_folder_number))
        os.makedirs(output_subfolder_path, exist_ok=True)
        
        for npy_file in npy_files:
            npy_file_path = os.path.join(subfolder_path, npy_file)
            
            # Load the .npy file
            data = np.load(npy_file_path)
            
            # Add noise to the data
            augmented_data = add_noise(data, noise_level)
            
            # Save the augmented data to the new output folder
            output_npy_file_path = os.path.join(output_subfolder_path, npy_file)
            np.save(output_npy_file_path, augmented_data)

def main():
    base_folder_path = r'C:\SKOLA\DIZERTACKA-Final\SW\Rokoko'
    output_base_path = r'C:\SKOLA\DIZERTACKA-Final\SW\Rokoko3'

    gesture_folders = [f for f in os.listdir(base_folder_path) if os.path.isdir(os.path.join(base_folder_path, f))]

    for gesture_folder in gesture_folders:
        gesture_folder_path = os.path.join(base_folder_path, gesture_folder)
        print(f"Processing gesture folder: {gesture_folder_path}")
        process_gesture_folder(gesture_folder_path, os.path.join(output_base_path, gesture_folder))
        
if __name__ == "__main__":
    main()
