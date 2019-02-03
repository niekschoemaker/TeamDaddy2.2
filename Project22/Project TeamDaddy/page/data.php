<link rel="stylesheet" type="text/css" href="css/data.css">
<?php
	//Redirect when not logged in
	// if(!isset($_SESSION['login'])){
	// 	header('Location: login.php');
	// }

	include 'navbar.php';
	
	header("Refresh: 60;");

	//mock data1
	$dataset1=array();
	//mock data2
	$dataset2=array();

	//Data for graph
	$dataPoints = array(
		array("label"=> "Jan", "y"=> array(4, 8)),
		array("label"=> "Feb", "y"=> array(3, 8)),
		array("label"=> "Mar", "y"=> array(5, 11)),
		array("label"=> "Apr", "y"=> array(8, 18)),
		array("label"=> "May", "y"=> array(12, 20)),
		array("label"=> "Jun", "y"=> array(17, 26)),
		array("label"=> "Jul", "y"=> array(19, 28)),
		array("label"=> "Aug", "y"=> array(19, 28)),
		array("label"=> "Sep", "y"=> array(16, 25)),
		array("label"=> "Oct", "y"=> array(12, 19)),
		array("label"=> "Nov", "y"=> array(9, 14)),
		array("label"=> "Dec", "y"=> array(6, 10))
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
		text: "Temperature over the stations"
	},
	axisY: {
		title: "Temperature (in °C)"
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
		name: "Temperature Range (min/max)",
		showInLegend: true,
		toolTipContent: "{label}<br><span style=\"color:#6D77AC\">{name}</span><br>Min: {y[1]} °C<br>Max: {y[0]} °C",
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
		name: "Average",
		showInLegend: true,
		markerType: "triangle",
		markerSize: 0,
		yValueFormatString: "##.0 °C",
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
</body>
</html>