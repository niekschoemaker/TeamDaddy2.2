<?php 
    session_start();
	session_unset();

	header('Location: login.php');
?>

<!DOCTYPE html>
<html>
<head>
	<title>Homepage</title>
	<link rel="stylesheet" type="text/css" href="css/index.css">
</head>
<body>
	<div>
		<h1>Hello world</h1></br>A <strong>beautiful</strong> world</br>
	</div>
</body>
</html>