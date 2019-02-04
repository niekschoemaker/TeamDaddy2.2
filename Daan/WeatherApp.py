import matplotlib.pyplot as plt
import matplotlib.animation as animation
from matplotlib import style
import matplotlib.ticker as mtick
import time
from datetime import datetime
import matplotlib.dates as mdates
import Testing
# Graph
style.use('ggplot')

fig = plt.figure(figsize=(10,6), dpi=100)
ax1 = fig.add_subplot(211)

def animate(i):
    # Load data from API (kunnen al laden uit voorbeeld protobuf bestand)
    graph_data = Testing.CloudCoverthread("jan mayen")
    xs = []
    for x in graph_data.values():
        xs.append(x)
    ys = []
    
    times = datetime.utcnow().strftime("%H:%M:%S")
    ys.append(times)
    

    # Set labels and titles
    
    ax1.plot(xs, ys)
    print(xs)
    print(ys)
    ax1.yaxis.set_major_formatter(mtick.PercentFormatter())
    plt.gca().xaxis.set_major_formatter(mdates.DateFormatter('%d/%m/%Y %H:%M:%S'))
    plt.gcf().autofmt_xdate()

    title = 'Cloud Cover Graph \n last updated: ' + time.strftime('%d/%m/%Y %H:%M:%S')
    plt.title(title)
    plt.xlabel("timestamp")
    plt.ylabel('cloud cover')
    plt.tight_layout()
    
ani = animation.FuncAnimation(fig, animate, interval=10000) # update elke 10 seconde
plt.show();