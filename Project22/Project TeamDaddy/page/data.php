<link rel="stylesheet" type="text/css" href="css/data.css">
<?php
	include 'navbar.php';

	echo "<table border=1px>";
	echo "<tr><td>stationid</td><td>temprature</td><td>humidity</td><td>windspeed</td></tr>";
	for ($i=0; $i < 100; $i++) { 
		echo('<tr>');
		$temp = rand(0,3);
		$stationid = rand(1024,102400);
		$humidity = rand(10,20);
		$windspeed = rand(15,20);
		$message = '<td>' . $stationid . '</td><td>' . $temp . '</td><td>' . $humidity . '</td><td>' . $windspeed . '</td></tr>';
		echo $message;
	}
?>
</body>
</html>