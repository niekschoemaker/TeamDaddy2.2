<!DOCTYPE html>
<html>
<head>
	<title>Login - TeamDaddy2.2</title>
	<link rel="stylesheet" type="text/css" href="css/login.css">
</head>
<body>
<!--login-->
<?php 
	include 'navbar.php';
?>
<form action="data.php" method="post">
	<div class="container">
		<label for="Username"><strong>Username</strong></label>	
		<input type=text placeholder="Username" id="Username" required>

		<label for="Password"><strong>Password</strong></label>	
		<input type=password placeholder="Password" id="Password" required>

		<button type="submit">Login</button>
	</div>
</form>
</body>
</html>