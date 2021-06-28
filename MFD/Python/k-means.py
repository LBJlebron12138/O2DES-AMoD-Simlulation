import xml.etree.ElementTree as ET
import numpy as np
import matplotlib.pyplot as plt
from sklearn.cluster import KMeans


#从xml文件中读取，用getroot获取根节点，根节点也是Element对象
tree = ET.parse('D:\\python\\network_berlin_fix.xml')
root = tree.getroot()
nodes=root[0]
data=np.empty([8060,3],dtype=float)
j=0
for node in nodes:
    data[j]=(float(node.attrib['id']),float(node.attrib['x']),float(node.attrib['y']))
    j=j+1
    #plt.scatter(float(node.attrib['x']),float(node.attrib['y']),s=2)
#print(data)  
clf=KMeans(n_clusters=29)
clf.fit(data)
centers=clf.cluster_centers_
labels=clf.labels_

with open("D:\\python\\node.txt",'w') as f:
   for i in range(len(labels)):
       f.write("{},{},{},{}\n".format(data[i,0],data[i,1],data[i,2],labels[i]))
       
    


#print(len(labels))

#colors={0:"brown",1:"blue",2:"cyan",3:"yellow",4:"black",5:"r",6:"sandybrown",7:"orange",8:"c",9:"m",10:"pink",11:"dodgerblue",12:"slategrey",13:"olive",14:"y",15:"skyblue",16:"palegreen",17:"deeppink",18:"plum",19:"wheat",20:"blueviolet",21:"beige",22:"chartreuse",23:"tomato",24:"gold",25:"salmon",26:"orchid",27:"tan",28:"coral",29:"aqua"}

#for l in range(len(labels)):
#    plt.scatter(data[l,0],data[l,1],s=1,c=colors[labels[l]])

    


#print(labels)
    #plt.plot()
    #print(node.attrib['x'],node.attrib['y'])
#plt.show()
#plt.savefig('D:\\python\\sandiantu.svg')
#print(nodes.tag)
    
#打印根节点的标签和属性
#for child in root:
#    print(child.tag, child.attrib)




