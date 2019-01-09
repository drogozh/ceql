drop database if exists `ceql`;
CREATE DATABASE `ceql`;

USE `ceql`;

CREATE TABLE `CUSTOMER` (
	CUSTOMER_ID INT AUTO_INCREMENT PRIMARY KEY,
    CUSTOMER_TYPE_ID INT NULL,
    TYPE_CD VARCHAR(64),
    TYPE_DESC VARCHAR(512),
    CREATE_TS TIMESTAMP
);


CREATE TABLE `ORDER` (
	ORDER_ID INT AUTO_INCREMENT PRIMARY KEY,
    CUSTOMER_ID INT NOT NULL,
    USER_ID INT NULL,
    CREATE_TS TIMESTAMP
);