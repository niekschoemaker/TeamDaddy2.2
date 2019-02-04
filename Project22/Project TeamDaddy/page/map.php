<?php
	session_start();

	if(!isset($_SESSION['login'])){
		header('Location: login.php');
	}

	$apikey = 'AIzaSyB9h1UbkBdHO94Lkl-3vCMCpdIDeIKP_nA';

	include 'navbar.php';
	include 'getall.php';

	$stations = genStations();
?>

<!DOCTYPE html>
<html>
	<head>
		<title>Map - TeamDaddy</title>
		<link rel="stylesheet" type="text/css" href="css/map.css">
		<meta name="viewport" content="initial-scale=1.0">
		<meta charset="utf-8">
	</head>
	<body>
		<div class="container">
			<div id="map"></div>
			<script>
				var map;
				function initMap() {
				map = new google.maps.Map(document.getElementById('map'), {
					center: {lat: 49.1972923, lng: 16.6039352},
					zoom: 8
				});

				var infowindow = new google.maps.InfoWindow({
					content:"Hello World!"
				});

				//marker 1
				var marker = new google.maps.Marker({
					position: {lat: 49.1972923, lng: 16.6039352},
					map: map,
					icon: { url: "https://www.clipartmax.com/png/middle/169-1692577_koko-%40-china-on-twitter-do-u-know-da-wae-sticker.png", scaledSize: new google.maps.Size(50, 50), origin: new google.maps.Point(0,0), anchor: new google.maps.Point(0, 0)},
					title: 'University',
					info: 'This marker is on the University <a href="station.php?station_id=1">More info</a>'
				});

				google.maps.event.addListener(marker, 'click', function() {
					infowindow.close();
					infowindow.setContent(this.info);
					infowindow.open(map, this);
				});


				//TODO Create marker generator
				//Import list of stations and ID's
				stations = <?php print(json_encode($stations)); ?>;
				//Get data from these stations
				for (var i = stations.length - 1; i >= 0; i--) {
					//Create marker
					var marker = new google.maps.Marker({
						position: {lat: parseInt(stations[i][3], 10), lng: parseInt(stations[i][4], 10)},
						map: map,
						title: stations[i][1],
						info: "Name: "+stations[i][1]+" </br> Country: "+stations[i][2]+"</br> Latitude: "+stations[i][3]+"</br> Longitude: "+stations[i][4]+"</br> Elevation: "+stations[i][5]+"</br><a href='station.php?station_id="+stations[i][1]+"'>More info</a>"
					})
					//Add info to the text box
					google.maps.event.addListener(marker, 'click', function() {
						infowindow.close();
						infowindow.setContent(this.info);
						infowindow.open(map, this);
					});
					//Repeat
				}
			}
			</script>
			<script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyB9h1UbkBdHO94Lkl-3vCMCpdIDeIKP_nA&callback=initMap"
			async defer></script>
		</div>
	</body>
</html>