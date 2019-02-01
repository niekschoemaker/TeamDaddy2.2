<?php 
//Set username password
$users = array(
	'user'=>'user',
	'admin'=>'admin'
);

//Session info
session_start();

if(isset($_SESSION['login'])){
	header('Location: data.php');
}else{
//login script
	if(isset($_POST['Username']) && isset($_POST['Password'])){
		if($users[$_POST['Username']] == $_POST['Password']){
			$_SESSION['login'] = true;
			header('Location: data.php');
		}
	}
}

?>

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
<form action="login.php" method="post">
	<div class="container">
		<label for="Username"><strong>Username</strong></label>	
		<input type=text placeholder="Username" name="Username" required>

		<label for="Password"><strong>Password</strong></label>	
		<input type=password placeholder="Password" name="Password" required>

		<button type="submit">Login</button>
	</div>
</form>
</body>
</html>