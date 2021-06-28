import pandas as pd 
import matplotlib.pyplot as plt
import time as time
import datetime 
import numpy as np 
from scipy.optimize import curve_fit

data_All=pd.read_csv("G:\\AMOD\\MFD_data\\MFDRecord_NIGHT18.csv",index_col=None,header=None,names=['K','V','Q'])
data=data_All[362:529]
K=1000*data["K"].values
V=3.6*data["V"].values
Q=3600*data["Q"].values

# #################################拟合优度R^2的计算######################################
def __sst(y_no_fitting):
    """
    计算SST(total sum of squares) 总平方和
    :param y_no_predicted: List[int] or array[int] 待拟合的y
    :return: 总平方和SST
    """
    y_mean = sum(y_no_fitting) / len(y_no_fitting)
    s_list =[(y - y_mean)**2 for y in y_no_fitting]
    sst = sum(s_list)
    return sst


def __ssr(y_fitting, y_no_fitting):
    """
    计算SSR(regression sum of squares) 回归平方和
    :param y_fitting: List[int] or array[int]  拟合好的y值
    :param y_no_fitting: List[int] or array[int] 待拟合y值
    :return: 回归平方和SSR
    """
    y_mean = sum(y_no_fitting) / len(y_no_fitting)
    s_list =[(y - y_mean)**2 for y in y_fitting]
    ssr = sum(s_list)
    return ssr


def __sse(y_fitting, y_no_fitting):
    """
    计算SSE(error sum of squares) 残差平方和
    :param y_fitting: List[int] or array[int] 拟合好的y值
    :param y_no_fitting: List[int] or array[int] 待拟合y值
    :return: 残差平方和SSE
    """
    s_list = [(y_fitting[i] - y_no_fitting[i])**2 for i in range(len(y_fitting))]
    sse = sum(s_list)
    return sse


def goodness_of_fit(y_fitting, y_no_fitting):
    """
    计算拟合优度R^2
    :param y_fitting: List[int] or array[int] 拟合好的y值
    :param y_no_fitting: List[int] or array[int] 待拟合y值
    :return: 拟合优度R^2
    """
    SSR = __ssr(y_fitting, y_no_fitting)
    SST = __sst(y_no_fitting)
    rr = SSR /SST
    return rr

def f_1(x, A, B):
 return A * x + B
def f_2(x,A,B,C):
 return A*x**2+B*x+C
stop=80
A, B = curve_fit(f_1, K[0:stop],V[0:stop])[0]
#A,B,C=curve_fit(f_2,K[0:stop],Q[0:stop])[0]

x=K
y=A*x+B
#x=K[0:65]
#y=A1*x+B1
plt.plot(x[0:stop],y[0:stop],"blue")
rr=goodness_of_fit(y[0:stop],V[0:stop])
print(rr)

plt.scatter(K[0:stop],V[0:stop],8,"red")
plt.grid()
plt.xlabel("Density(Veh/Km)",fontsize=15)
plt.ylabel("Speed(Km/H)",fontsize=15)
plt.show()