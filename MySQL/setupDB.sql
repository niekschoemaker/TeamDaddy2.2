/*
	Author: Daan Aalders & Sean Visser
	Studentnumbers: 373884 & 356343
	Date: 09-12-2018 (DD-MM-YYYY)
	Class: ITV2A
	
	Description:
		MySQL script for creating the United Nations Weather Data Management Institution(UNWDMI).
		
		This script will create one new database, three new tables and two new users.
		
		The database will be named 'Weermeting', and contains the following three tables with their required collumns:
			
			station:
			
				- stn #Unique ID corresponding to the weather station. Note: !FOREIGN KEY!
				- name #Name of the weather station.
				- latitute #Latitude of the weather station.
				- longitude #Longitude of the weather station.
				- country #The country that the weather station is in.
				- timezone #The local timezone that the weather station is in.
			
			timezone:
				- timezone_ID #This is the timezone ID. Example: UTC
				- gmt_offset #This is the offset of the timezone_ID compared to GMT. Example: UTC is GMT +0
				- dst #???? Misschien: station.stn(?)
			
			measurements:
				
				Daan, vul deze in AUB :3
				
				
		
		There will be three users. One for Raspberry-level acces, one for user-level acces and one for admin-level acces.
			
			Raspberry:
			
				'weerberry'@'%'
				
			User:
			
				'weeruser'@'%'
				
			Admin:
			
				'weerdaddy'@'localhost'
		
		
	Security Measurements:
	
		- All connections go over SSL to prevent eavedroppers and data breaches; 
	
		- Raspberry can only write(insert) to Weermeting.measurements;
		- Raspberry password uses the sha256_password authentication method;
		- Raspberry can have 10 simultaneous connections; One for every Raspberry MySQL-Connection Thread;
		- Raspberry uses SSL to send data to the database;
	
		- User needs to update his password every 180 days;
		- User password uses the sha256_password authentication method;
		- User only has access to the 'Weermeting' database and its tables;
		- User can only read data from the 'Weermeting' database;
		- User can have 13 simultaneous connections; 10 for the Data Acquisition department, 1 for the direction department, 1 for the IT department, 1 for overhead
		
		- Admin needs to update his password every 90 days;
		- Admin password uses the sha256_password authentication method;
		- Admin only has access to the 'Weermeting' database;
		- Admin has full acces to the 'Weermeting' database;
		- Admin can only connect from localhost;
		- Admin can have 2 simultaneous connections; 1 for the IT department, 1 for overhead
*/


/*
	Part one: Creating the database and tables
*/

	/*
		Settting up database
	*/
DROP DATABASE IF EXISTS Weermeting;
CREATE DATABASE Weermeting;

	/*
		Setting up the tables
	*/

	--- TODO: Daan


	
/*
	Part two: Creating the user-level and admin-level acces accounts with corresponding security measurements
*/
	
	/*
		Setting up the weatherstation-level acces user
	*/
	
DROP USER IF EXISTS 'weerberry'@'%';
CREATE USER IF NOT EXISTS 'weerberry'@'%' IDENTIFIED WITH sha256_password BY 'TeamD420Station' PASSWORD EXPIRE NEVER WITH MAX_USER_CONNECTIONS 10;
GRANT INSERT ON Weermeting.measurements to 'weerberry'@'%' REQUIRE SSL;

	/*
		Setting up the user-level acces user
	*/
	
DROP USER IF EXISTS 'weeruser'@'%';
CREATE USER IF NOT EXISTS 'weeruser'@'%' IDENTIFIED WITH sha256_password BY 'TeamD69User' PASSWORD EXPIRE INTERVAL 180 DAY WITH MAX_USER_CONNECTIONS 13;
GRANT SELECT ON Weermeting.* to 'weeruser'@'%' REQUIRE SSL;

	/*
		Setting up the admin-level acces account
	*/
DROP USER IF EXISTS 'weerdaddy'@'localhost';
CREATE USER IF NOT EXISTS 'weerdaddy'@'localhost' IDENTIFIED WITH sha256_password BY 'TeamD1337Daddy' PASSWORD EXPIRE INTERVAL 90 DAY WITH MAX_USER_CONNECTIONS 2;	
GRANT ALL ON Weermeting.* to 'weerdaddy'@'localhost' REQUIRE SSL;




