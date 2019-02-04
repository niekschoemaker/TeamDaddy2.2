import Testing
import cgi, cgitb
# import nltk
# from nltk.tokenize import sent_tokenize, word_tokenize
import sys


def Windspeed(station):
    w = Testing.Windspeedthread(station)
    for x in w.values():
        print(x)
    for x in w:
        print(x)
    return

#print("Content-type:text/html\n")
#print("hello")
text_content = ''
for word in sys.argv[1:]:
    text_content += word + ' '



Windspeed("Tak")
#toptien(text_content)