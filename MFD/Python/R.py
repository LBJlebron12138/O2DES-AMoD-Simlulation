import matplotlib.pyplot as plt
from matplotlib.pyplot import MultipleLocator
import matplotlib as mpl
from matplotlib.font_manager import FontProperties
import pandas as pd
import numpy as np
font_TNR=FontProperties(fname=r"C:\\Windows\\Fonts\\times.ttf",size=12)
font_song=FontProperties(fname=r"C:\\Windows\\Fonts\\simsun.ttc",size=12)
mpl.rcParams['font.sans-serif'] = ['SimHei']
'''
Vsize=[6000,7000,8000,9000,10000,11000,12000,13000,14000,15000,16000,17000]

#Timeold=[192.359093105556,190.252091569048,188.13233155,186.009086535185,182.612061778333,176.493047974242,171.723075269444,162.044214801282,154.228998325,147.246236175556,140.297296833333,135.344498610784]
#
# Timenew=[198.359093105556,193.252091569048,191.13233155,189.009086535185,186.612061778333,181.493047974242,176.723075269444,166.044214801282,158.228998325,150.246236175556,142.297296833333,136.344498610784]
Cancelold=[45877,37018,28954,21993,16640,12422,8965,6339,4382,2721,1436,270]
Cancelnew=[44414,36468,27055,20417,15440,11422,8065,5539,3782,2321,1236,100]
plot1=plt.plot(Vsize,Cancelold,color='blue',linewidth=1.0,marker='.',label="全局二部图匹配")
plot1=plt.plot(Vsize,Cancelnew,color='orange',linewidth=1.0,marker='.',label="改进策略")
plt.legend(fontsize=12)
plt.xlabel("车队规模")
plt.ylabel("订单取消数")
plt.show()
'''


node1_now=pd.read_csv("F:\\发送文件\\发送文件\\绘图文件\\绘图文件\\Node1当前.csv",index_col=None,header=None,names=['time','actvity','num',"speed"])
node1_close=pd.read_csv("F:\\发送文件\\发送文件\\绘图文件\\绘图文件\\Node1近期.csv",index_col=None,header=None,names=['time','actvity','num',"speed"])
node1_far=pd.read_csv("F:\\发送文件\\发送文件\\绘图文件\\绘图文件\\Node1远期.csv",index_col=None,header=None,names=['time','actvity','num',"speed"])

list1=[node1_now,node1_close,node1_far]
list2=[]
list3=[]
i=0
for node in list1:
  time1=node["time"].values
  num1=node["num"].values


  for i in range(time1.shape[0]):
   time1[i]=time1[i].split()[1]
   time1[i]=float(time1[i].split(":")[0])*60+float(time1[i].split(":")[1])+float(time1[i].split(":")[2])/60

  for i in range(time1.shape[0]):
    num1[i]=int(num1[i])
  list2.append(time1)
  list3.append(num1)
  i+=1
  


#y_major_locator=MultipleLocator(50)
plt.figure()
ax=plt.gca()
#ax.yaxis.set_major_locator(y_major_locator)

#plt.subplot(311)

plt.grid(axis="x")
plt.plot(list2[0],list3[0])
plt.title("当前")
plt.ylim(150,450)
plt.style.use("seaborn")
plt.show()
'''
plt.subplot(312)
plt.style.use("seaborn")
plt.grid(axis="x")
plt.plot(list2[1],list3[1])
plt.xlabel("时间(min)")
plt.ylabel("通道顾客数量")
plt.ylim(150,450)
plt.subplot(313)
plt.plot(list2[2],list3[2])

plt.ylim(150,450)
plt.grid(axis="x")

plt.style.use("seaborn")
plt.show()
'''