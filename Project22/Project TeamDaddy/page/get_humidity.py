import Testing
import cgi, cgitb
# import nltk
# from nltk.tokenize import sent_tokenize, word_tokenize
import sys


def humidity(station):
    h = Testing.Humiditythread(station)
    for elke in h.values():
        print(elke)
    for elke in h:
        print(elke)
    return

#print("Content-type:text/html\n")
#print("hello")
text_content = ''
for word in sys.argv[1:]:
    text_content += word + ' '



#humidity(text_content) #moet toptien(text_content) worden
humidity(text_content)