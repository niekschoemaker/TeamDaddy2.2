<?php
	session_start();

	if(!isset($_SESSION['login'])){
		header('Location: login.php');
	}

	$apikey = 'AIzaSyB9h1UbkBdHO94Lkl-3vCMCpdIDeIKP_nA';

	include 'navbar.php';
?>

<!DOCTYPE html>
<html>
	<head>
		<title>Simple Map</title>
		<meta name="viewport" content="initial-scale=1.0">
		<meta charset="utf-8">
		<style>
			/* Always set the map height explicitly to define the size of the div
			* element that contains the map. */
			#map {
				height: 100%;
			}
			/* Optional: Makes the sample page fill the window. */
			html, body {
				height: 100%;
				margin: 0;
				padding: 0;
			}

			.container{
				margin: auto;
				width: 90vw;
				height: 90vh;
			}
		</style>
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
					title: 'University',
					info: 'This marker is on the University <a href="station.php?station_id=1">More info</a>'
				});

				google.maps.event.addListener(marker, 'click', function() {
					infowindow.close();
					infowindow.setContent(this.info);
					infowindow.open(map, this);
				});

				//Marker 2
				var marker = new google.maps.Marker({
					position: {lat: 50.1972923, lng: 17.6039352},
					map: map,
					title: 'test marker',
					info: 'Marker 2 is a test marker'
				});

				google.maps.event.addListener(marker, 'click', function() {
					infowindow.close();
					infowindow.setContent(this.info);
					infowindow.open(map, this);
				});


				//TODO Create marker generator
				//Import list of stations and ID's
				stations = [...Array(8000).keys()];
				//Get data from these stations
				for (var i = stations.length - 1; i >= 0; i--) {
					stations[i] = [Math.floor((Math.random() * 100) + 1) /*Temprature*/, Math.floor((Math.random() * 100) + 1)/*Windspeed*/, Math.floor((Math.random() * 100) + 1)/*Humidity*/, [(Math.random() * 170) - 85/*latitude*/, (Math.random() * 300) - 150]/*longitude*/];
					//Create marker
					var marker = new google.maps.Marker({
						position: {lat: stations[i][3][0], lng: stations[i][3][1]},
						map: map,
						title: toString(i),
						info: "This is marker "+i+"</br> Temprature: "+stations[i][0]+"</br> Windspeed: "+stations[i][1]+"</br> Humidity: "+stations[2]+"</br> At: "+stations[i][3][0] +", "+stations[i][3][1]+" </br><a href='station.php?station_id="+i+"'>More info</a>"
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