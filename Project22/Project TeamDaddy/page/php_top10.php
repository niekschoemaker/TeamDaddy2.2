<?php
// define variables and set to empty values
function getTopTen(){
$text_content="";
$hello="";
//if ($_SERVER["REQUEST_METHOD"] == "GET") {

//$hello = $_GET['text_content'];
$top10 = array();

$command="python get_top10.py ".$hello;
exec($command , $out,$ret );
#echo $ret;
#echo $out;
$i = 0;
foreach ($out as $line){
    $top10[$i] = explode(" ", $line);
    $i++;
	}
 // }
  return($top10);
}
?>