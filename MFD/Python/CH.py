import pandana as pdna
import pandas as pd
import xml.etree.ElementTree as ET
import numpy as np
import matplotlib.pyplot as plt
import time


def ConstructRoutingTables():
  df_node.sort_index()
  a=0
  for n in df_node:
    df_node[n]["index"]=a
    a+=1
  index=0
  for index in df_node.shape[0]:
    Sinks=df_node.values["index"].
    source=index



   

#从xml文件中读取，用getroot获取根节点，根节点也是Element对象
tree = ET.parse('D:\\network_berlin_fix.xml')
root = tree.getroot()
Nodes=root[0]
Links=root[1]

i=0
node_data=np.empty([8060,4])

for node in Nodes:

    node_data[i]=(int(node.attrib["id"]),float(node.attrib["x"]),float(node.attrib["y"]),i)
    i+=1
df_node=pd.DataFrame(data=node_data[:,1:],index=node_data[:,0],columns=('x','y','index'))

j=0
link_data=np.empty([16016,4])
for Link in Links:
    link_data[j]=(j,float(Link.attrib["from"]),float(Link.attrib["to"]),float(Link.attrib["length"]))
    j+=1
list1=df_node["index"].values
print(list1.dtpye)
'''
df_link=pd.DataFrame(data=link_data[:,1:],index=link_data[:,0],columns=('from','to','length'))


net=pdna.Network(df_node["x"],df_node["y"],df_link["from"],df_link["to"],df_link[["length"]])

start=time.time()
nodes_a=df_node.index.values[0:2].tolist()
nodes_b=df_node.index.values[2:4].tolist()


SP=net.shortest_paths(nodes_a,nodes_b)
end=time.time()
gap=end-start
print(SP)
print(gap)
#print(SP)
'''
