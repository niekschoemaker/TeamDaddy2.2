<!DOCTYPE html>
<html>
<head>
	<title>Station <?php print($_GET['station_id']) ?></title>
	<style type="text/css">
		#header{
			text-align: center;
		}
	</style>
</head>
<body>

<?php 
//Get information of ?
if(isset($_GET['station_id'])){
	$stationID = $_GET['station_id'];
	print('<div id="header"><h1>'.$stationID.'</h1></div>');
	$station = array(array());
	print('<table>');
	for ($i=0; $i < sizeof($station[0]); $i++) { 
		print('
			<tr>
				<td>'.$station[0][$i].'</td>
				<td>'.$station[1][$i].'</td>
				<td>'.$station[2][$i].'</td>
			</tr>
		');
	}
	print('</table>');
}else{
	print('<div><h1>Station does not exist</h1></div>');
}

?>
<table>
	<tr>
		<td></td>
		<td></td>
		<td></td>
	</tr>
</table>

</body>
</html>