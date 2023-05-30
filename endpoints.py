from flask import Flask, request, jsonify
import numpy as np
from load_model import load_model
import pandas as pd
from data_retriever import retriever

# CLASSES #

class GuestData:
    def __init__(self, guest_count, dates):
        self.guest_count = guest_count
        self.dates = dates
class Dataframe:
    def __init__(self, dataframe):
        self.dataframe = dataframe

app = Flask(__name__)
# Run the initialization method before the first request

def initialize():
    # Assign SCALER, MODEL to app.config
    scaler, model = load_model()
    app.config['SCALER'] = scaler 
    app.config['MODEL'] = model

@app.before_first_request
def before_first_request():
    initialize()
    
@app.route('/predict', methods=['POST'])
def predict():
    scaler = app.config['SCALER']
    model = app.config['MODEL']
    
    input_data = request.json['data']  # Assuming input data is provided as a JSON object with 'data' key
    input_data = np.array(input_data)  # Convert input data to numpy array
    input_data = np.reshape(input_data, (-1, 8))  # Reshape with rows wildcard (-1) and 8 columns

    # Normalize input data using the scaler
    input_data_scaled = scaler.transform(input_data)
    prediction = model.predict(input_data_scaled)
    pred_item = prediction.tolist()[0]
    response = {'guest_ct' : pred_item}

    return jsonify(response)

# Returns predictions in range of n days in the past
# along with the actual guest count that date
@app.route('/get-pred-range', methods=['GET'])
def pred_range():
    ret = retriever()

    scaler = app.config['SCALER']
    model = app.config['MODEL']
    days_back = request.args.get('days_back')
    
    # Obtain data from data loader
    # and select specific columns to use
    data = ret.get_full_data(int(days_back))
    guest_ct, date = ret.get_data(int(days_back))
    selected_columns = ['temp', 'precip_hours', 'holiday_flag', 'is_weekend', 'is_weekday', 'yesterday', 'last_7_days', 'last_28_days']
    input_data = data[selected_columns].copy()
    
    # Scale input and create predictions for each row
    scaled_input = scaler.transform(input_data)
    dates = data['arrival_date'].to_list()
    input_data = scaled_input
    preds = model.predict(input_data).ravel().tolist()
    
    # Return prediction, date and actual guest count
    response = {'pred' : preds, 'date': dates, 'guest_ct': guest_ct.tolist()}

    return jsonify(response)

# Returns only the actual guest count for 
# n days in the past
@app.route('/guest-hist', methods=['GET'])
def guest_hist():
    ret = retriever()

    # Get data and return date, guest count
    days_back = request.args.get('days_back')
    guest_ct, date = ret.get_data(int(days_back))
    response = {'guest_ct' : guest_ct.to_list(), 'date': date.to_list()}

    return jsonify(response)

# Returns factors that most strongly correlate
# to the guest count feature
@app.route('/data-cor', methods=['GET'])
def data_cor():
    ret = retriever()
    cor = ret.get_correlations()
    cor_dict = {}

    # Loop over each row, unpack into correlation dict
    for index, row in cor.iterrows():
        print(row)
        name = row['name']
        cor = row['cor']
        cor_dict[name] = cor
    
    # Return dict
    return jsonify(cor_dict)

# Retrieve subset of the full dataset as raw data
# to be displayed in the dataframe panel
@app.route('/data-full', methods=['GET'])
def data_full():
    ret = retriever()
    days_back = request.args.get('days_back')

    data = ret.get_full_data(int(days_back))
    response = \
    {
        'guest_ct' : data['total_guests'].to_list(), 
        'date': data['arrival_date'].to_list(),
        'temp': data['temp'].to_list(),
        'holiday_flag': data['holiday_flag'].to_list(),
        'weekend_flag': data['is_weekend'].to_list(),
        'weekday': data['is_weekday'].to_list(),
        'last_7': data['last_7_days'].to_list(),
        'last_28': data['last_28_days'].to_list(),
    }

    return jsonify(response)


if __name__ == '__main__':
    app.run()