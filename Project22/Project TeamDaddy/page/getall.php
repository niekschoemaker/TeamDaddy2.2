<?php
// define variables and set to empty values
function genStations(){

	$text_content="";
	$hello="";
	$stations = array();

	if ($_SERVER["REQUEST_METHOD"] == "GET") {

	//$station = $_GET['text_content'];
	$command="python positie.py";
	exec($command , $out,$ret );
	#echo $ret;
	#echo $out;

	$i=0;
	$j=0;
	foreach ($out as $line){
		$stations[$j][$i] = $line;
		$i++;
		if($i == 6){
			$j++;
			$i = 0;
		}
	}
	}
	return($stations);
	print($stations[1][2].", ".$stations[2][2]);
}

?>