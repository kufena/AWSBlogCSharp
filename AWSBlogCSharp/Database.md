# Database Model

Using a MySQL database, create a specific user and database for the application.
These are named in the connection string, which you put in a secret.

Then the schema looks like this, at the moment:

    create database GateHouseWerehamBlog;
    use GateHouseWerehamBlog;

    create table BlogIds (Id int auto_increment primary key, Fake varchar(10));
    create table BlogPost (Id int not null, Version int not null, Title varchar(2048), 
                           Date datetime, File varchar(1024), Status enum('True','False'), Hash varchar(1024), key (Id,Version));
