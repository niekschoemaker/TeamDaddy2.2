from django.shortcuts import render
from django.http import HttpResponse
import time

def index(request):
	return render(request=request, template_name="homepage/home.html", context={"date": time.strftime("%H:%M:%S")})
	# HttpResponse("<style>body{background:#3c3c3c;color:#fff;font-size:3em}h1{margin:0;}div{ margin:0; position:absolute; left:50vw; top:50vh; transform:translate(-50%, -50%); text-align:center; line-height:.6 }strong{color:#17a2b8}</style><div><h1>Hello world</h1></br>A <strong>beautiful</strong> world</div>")
# Create your views here.