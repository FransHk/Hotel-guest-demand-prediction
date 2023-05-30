import numpy as np
import plotly.express as px
import os
import pickle
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from flask import Flask, request, jsonify

def build_DNN_model():
  model = keras.Sequential([
      layers.Dense(64, activation='relu', input_shape=(8,)),
      layers.Dense(32, activation='relu'),
      layers.Dropout(rate=0.5),
      layers.Dense(1)
  ])

  model.compile(loss='mean_absolute_error',
                optimizer=tf.keras.optimizers.Adam(0.001))
  return model

def load_model():
    scaler = None
    with open('model/scaler.pkl', 'rb') as f:
        scaler = pickle.load(f)
        
    new_model = build_DNN_model()
    new_model.build(input_shape=(8,))
    new_model.load_weights('model/model_weights.h5')
    return scaler, new_model




