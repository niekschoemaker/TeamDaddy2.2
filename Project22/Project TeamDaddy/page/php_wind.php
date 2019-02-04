<body>  
<?php
// define variables and set to empty values

$text_content="";
$hello="";
if ($_SERVER["REQUEST_METHOD"] == "GET") {

$station = $_GET['text_content'];


$command="python get_windspeed.py".$station;
exec($command , $out,$ret );
#echo $ret;
#echo $out;

foreach ($out as $line){
    print "$line\n";
	}

  }
?>

<form method="get" action="<?php echo htmlspecialchars($_SERVER["PHP_SELF"]);?>">  
 <textarea name="text_content" value="<?php echo $text_content;?>" cols="400" rows="4"> 
</textarea>

  <input type="submit" name="submit" value="Submit">  
</form>


</body>
</html>