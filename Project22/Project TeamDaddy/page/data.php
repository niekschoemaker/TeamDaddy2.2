<link rel="stylesheet" type="text/css" href="css/data.css">
<?php
	//Redirect when not logged in
	// if(!isset($_SESSION['login'])){
	// 	header('Location: login.php');
	// }

// define variables and set to empty values

// $text_content="";
$hello="Czech";
// if ($_SERVER["REQUEST_METHOD"] == "GET") {

	// $hello = $_GET['text_content'];


	$command="python get_top10.py ".$hello;
	exec($command , $out,$ret );
	#echo $ret;
	#echo $out;
	$i = 0;
	foreach ($out as $line){
		$data[$i] = $line;
		$i++;
	}

	include 'navbar.php';

	include 'php_top10.php';

	$top10 = getTopTen();
	
	header("Refresh: 60;");

	//mock data1
	$dataset1=array();
	//mock data2
	$dataset2=array();

	//Data for graph
	$dataPoints = array(
		array("label"=> $top10[0][0], "y"=> array(0, $top10[0][1])),
		array("label"=> $top10[2][0], "y"=> array(0, $top10[2][1])),
		array("label"=> $top10[4][0], "y"=> array(0, $top10[4][1])),
		array("label"=> $top10[6][0], "y"=> array(0, $top10[6][1])),
		array("label"=> $top10[8][0], "y"=> array(0, $top10[8][1])),
		array("label"=> $top10[10][0], "y"=> array(0, $top10[10][1])),
		array("label"=> $top10[12][0], "y"=> array(0, $top10[12][1])),
		array("label"=> $top10[14][0], "y"=> array(0, $top10[14][1])),
		array("label"=> $top10[16][0], "y"=> array(0, $top10[16][1])),
		array("label"=> $top10[18][0], "y"=> array(0, $top10[18][1]))
	);

	//Mock data used during FM2

	// echo "<table border=1px>";
	// echo "<tr><td>stationid</td><td>temprature</td><td>humidity</td><td>windspeed</td></tr>";
	// for ($i=0; $i < 100; $i++) { 
	// 	echo('<tr>');
	// 	$temp = rand(0,3);
	// 	$stationid = rand(1024,102400);
	// 	$humidity = rand(10,20);
	// 	$windspeed = rand(15,20);
	// 	$message = '<td>' . $stationid . '</td><td>' . $temp . '</td><td>' . $humidity . '</td><td>' . $windspeed . '</td></tr>';
	// 	echo $message;
	// }
?>

<!DOCTYPE HTML>
<html>
<head>
<script>
window.onload = function () {
 
var chart = new CanvasJS.Chart("chartContainer", {
	//theme: "light2",
	title: {
		text: "humidity over the stations"
	},
	axisY: {
		title: "humidity"
	},
	toolTip: {
		shared: true
	},
	legend: {
		dockInsidePlotArea: true,
		cursor: "pointer",
		itemclick: toggleDataSeries
	},
	data: [{
		type: "rangeArea",
		markerSize: 0,
		name: "",
		showInLegend: false,
		toolTipContent: "{label}<br><span style=\"color:#6D77AC\">{name}</span><br>Min: {y[1]} <br>Max: {y[0]} ",
		dataPoints: <?php echo json_encode($dataPoints, JSON_NUMERIC_CHECK); ?>
	}]
});
chart.render();
 
addAverages();
 
function addAverages() {
	var dps = [];
	for(var i = 0; i < chart.options.data[0].dataPoints.length; i++) {
		dps.push({
			label: chart.options.data[0].dataPoints[i].label,
			y: (chart.options.data[0].dataPoints[i].y[0] + chart.options.data[0].dataPoints[i].y[1]) / 2
		});
	}
	chart.options.data.push({
		type: "line",
		name: "",
		showInLegend: false,
		markerType: "triangle",
		markerSize: 0,
		yValueFormatString: "##.0 %",
		dataPoints: dps
	});
	chart.render();
}
 
function toggleDataSeries(e) {
	if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
		e.dataSeries.visible = false;
	} else {
		e.dataSeries.visible = true;
	}
	e.chart.render();
}
 
}
</script>
</head>
<body>
<div id="chartContainer">
<script src="https://canvasjs.com/assets/script/canvasjs.min.js"></script>
</div>

<table>
	<tr>
		<td><?php echo($top10[0][0]); ?></td>
		<td><?php echo($top10[0][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[1][0]); ?></td>
		<td><?php echo($top10[1][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[2][0]); ?></td>
		<td><?php echo($top10[2][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[3][0]); ?></td>
		<td><?php echo($top10[3][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[4][0]); ?></td>
		<td><?php echo($top10[4][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[5][0]); ?></td>
		<td><?php echo($top10[5][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[6][0]); ?></td>
		<td><?php echo($top10[6][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[7][0]); ?></td>
		<td><?php echo($top10[7][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[8][0]); ?></td>
		<td><?php echo($top10[8][1]); ?></td>
	</tr>
	<tr>
		<td><?php echo($top10[9][0]); ?></td>
		<td><?php echo($top10[9][1]); ?></td>
	</tr>

</table>
</body>
</html>