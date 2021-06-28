import pandas as pd 
import matplotlib.pyplot as plt
import time as time
import datetime 
import numpy as np 
from scipy.optimize import curve_fit


t1=datetime.datetime.now()
data_train=pd.read_csv("F:\\AMOD\\Area_time24.csv",index_col=None,header=None,names=['time','rate'])
data_test=pd.read_csv("F:\\AMOD\\Area_time16.csv",index_col=None,header=None,names=['time','rate'])
#data_1=data[655:3233]

list1=[]
def Dataprocess(data):
 data.insert(len(data.columns),'trans_time',np.ones(data.shape[0]))
 for index,row in data.iterrows():
    T=row['time'].split(":")
    a=datetime.timedelta(minutes=float(T[1]),seconds=float(T[2])).total_seconds()
    #a=a/60
    
    #row['trans_time']=a
    data.loc[index,'trans_time']=a
 return data

data_train=Dataprocess(data_train)
data_test=Dataprocess(data_test)

train=data_train[655:3233]
x=train["trans_time"].values
y=train["rate"].values

x_test=data_test[280:]["trans_time"].values
y_test=data_test[280:]["rate"].values


'''
def func(x,a,b):
    return a * np.log(x) + b

popt, pcov = curve_fit(func, x, y)
a=popt[0]#popt里面是拟合系数，读者可以自己help其用法
b=popt[1]
yvals=func(x,a,b)
'''
def func(x, a, b, c):
 return a * np.exp(-b * x) + c
#f1=np.polyfit(x,y,7)
p1=np.poly1d(f1)
yvals=p1(x_test)
plot1=plt.plot(x,y,'s',markersize=2)
plot2=plt.plot(x_test,yvals,'bo',markersize=2)
plt.show()
print(p1)
print(p1(10))







