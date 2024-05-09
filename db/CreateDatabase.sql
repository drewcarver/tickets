CREATE DATABASE IF NOT EXISTS Ticket;

USE Ticket;

CREATE TABLE Ticket (
  TicketId INT AUTO_INCREMENT PRIMARY KEY,
  Title VARCHAR(500) NOT NULL,
  Description VARCHAR(5000) NOT NULL,
  Status INT NOT NULL
);

CREATE USER 'ticketapp'@'localhost' IDENTIFIED BY 'ticket123';

GRANT ALL PRIVILEGES ON Ticket.* TO 'ticketapp'@'localhost';
