import Testing
import cgi, cgitb
# import nltk
# from nltk.tokenize import sent_tokenize, word_tokenize
import sys


def toptien(land):
    print(Testing.Toptenthread(land))
   #return Testing.Toptenthread("Czech")
    return

#print("Content-type:text/html\n")
#print("hello")
text_content = ''
for word in sys.argv[1:]:
    text_content += word + ' '



toptien(text_content) #moet toptien(text_content) worden