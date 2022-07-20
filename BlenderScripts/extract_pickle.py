import pickle
import json
from matplotlib.pyplot import switch_backend 

import numpy as np

def transform_to_lists(data_dict):
    res = {}
    for key, value in data_dict.items():
        if  type(value) is dict:
            res[key] = transform_to_lists(value)
        elif type(value) is np.ndarray:
            value = np.squeeze(value)
            temp_dict = {"shape": list(value.shape), "data": np.reshape(value, -1).tolist()}
            res[key] = temp_dict
        elif type(value) is list:
            res[key] = [transform_to_lists(x) for x in value]
        elif key == "curr_target_wpath":
            res[key] = {"index": value[0], "position": value[1].tolist()}
        else:
            res[key] = value

    return res
filepath = '/mnt/c/Users/Lukas/Projects/ar-population/Data/GammaResults/MPVAEPolicy_v0/res_000.pkl'
outpath = '/mnt/c/Users/Lukas/Projects/ar-population/Data/GammaResults/MPVAEPolicy_v0/res_000.json'
with open(filepath, "rb") as f:
        dataall = pickle.load(f, encoding="latin1")
        new_dict = transform_to_lists(dataall)
        json_object = json.dumps(new_dict, indent = 4) 
        with open(outpath, "w") as out:
            out.write(str(json_object))