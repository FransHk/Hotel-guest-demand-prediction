import numpy as np
from load_model import load_model
import pandas as pd

class retriever:
    def __init__(self) -> None:
        self.data = pd.read_csv('data/final/data.csv')
        self.cor_data = pd.read_csv('data/final/correlations.csv')

    def get_data(self, days_back):
        subset = self.data.iloc[-days_back:]
        return subset['total_guests'], subset['arrival_date']
    
    def get_correlations(self):
        return self.cor_data

    def get_full_data(self, days_back):
        subset = self.data.iloc[-days_back:]
        return subset


    
    