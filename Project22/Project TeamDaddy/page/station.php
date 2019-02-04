<!DOCTYPE html>
<html>
<head>
	<title>Station <?php if(isset($_GET['station_id'])){print($_GET['station_id']);} ?> - TeamDaddy</title>
	<link rel="stylesheet" type="text/css" href="css/station.css">
</head>
<body>
<?php

header("Refresh: 60;");

include 'getall.php';
include 'navbar.php';

$stations = genStations();

//print_r($stations);

//$stations = range(0, 8000);

//Get information of ?
if(isset($_GET['station_id'])){
	$stationID = $_GET['station_id'];
	print('<div id="header"><h1>'.$stationID.'</h1></div>');
	$station = array(array(), array(), array());
		$station[1] = Rand(10,100);
		$station[2] = Rand(-10,35);
		$station[3] = Rand(10,20);
	print('<table>
			<tr>
				<td>Windspeed</td>
				<td>Temprature</td>
				<td>Humidity</td>
			</tr>
		');
	for ($i=0; $i < 30; $i++) { 
		print('
			<tr>
				<td>'.$station[1]/*Windspeed*/.'</td>
				<td>'.$station[2]/*Temprature*/.'</td>
				<td>'.$station[3]/*Humidity*/.'</td>
			</tr>
		');
	}
	print('</table>');
}
print('<div><form action="" method="GET"><select name=station_id>');
for ($i=0; $i < sizeof($stations); $i++) { 
	print('<option>'.$stations[$i][1].'</option>');
}
print('</select><button type="submit">Go</button></form></div>');


?>

</body>
</html>