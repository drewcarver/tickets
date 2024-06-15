CREATE TABLE IF NOT EXISTS Ticket (
  TicketId INT PRIMARY KEY,
  Title TEXT NOT NULL,
  Description TEXT NOT NULL,
  Status INT NOT NULL
);

INSERT INTO Ticket (TicketId, Title, Description, Status) VALUES (1, 'Create UI', 'Create the UI', 1);