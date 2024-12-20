Create table Client
(
	ID INT PRIMARY KEY IDENTITY,
	fio NVARCHAR(255) NOT NULL,
	phone NVARCHAR(11) NOT NULL,
	logins NVARCHAR(30) NOT NULL,
	passwords NVARCHAR(30) NOT NULL,
	CONSTRAINT check_phone_format CHECK (phone LIKE '8[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')
)
Create table Staff
(
	ID INT PRIMARY KEY IDENTITY,
	fio NVARCHAR(255) NOT NULL,
	phone NVARCHAR(11) NOT NULL,
	logins NVARCHAR(30) NOT NULL,
	passwords NVARCHAR(30) NOT NULL,
	[type] NVARCHAR(20) NOT NULL,
	CONSTRAINT check_phone_format1 CHECK (phone LIKE '8[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')
)
Create table RequestStatus
(
	ID INT PRIMARY KEY IDENTITY,
	requestStatus NVARCHAR(255) NOT NULL
)
Create table Technic
(
	ID INT PRIMARY KEY IDENTITY,
	climateTechType NVARCHAR(255) NOT NULL,
	climateTechModel NVARCHAR(255) NOT NULL
)
Create table Requests
(
	ID INT PRIMARY KEY IDENTITY,
    startDate DATETIME,
	techincID INT,
	problemDescryption NVARCHAR(255) NOT NULL,
	requestStatusID INT,
	completionDate DATETIME,
	repairParts NVARCHAR(255),
	staffID INT,
	clientID INT,
	FOREIGN KEY (staffID) REFERENCES Staff(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (clientID) REFERENCES Client(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (requestStatusID) REFERENCES RequestStatus(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (techincID) REFERENCES Technic(ID) ON DELETE CASCADE ON UPDATE CASCADE,
)
CREATE TABLE Comments
(
    ID INT PRIMARY KEY IDENTITY,
    [message] NVARCHAR(255) NOT NULL,
    staffID INT,
    requestID INT,
    FOREIGN KEY (staffID) REFERENCES Staff(ID) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (requestID) REFERENCES Requests(ID) ON DELETE NO ACTION ON UPDATE NO ACTION
)
CREATE TABLE loginHistory (
    loginHistoryId INT PRIMARY KEY IDENTITY,
    logins NVARCHAR(30) NOT NULL,
    attemptDate DATETIME NOT NULL,
    success BIT NOT NULL
); 
INSERT INTO Staff 
VALUES
('Широков Василий Бульба','89210563128','login1','pass1','Менеджер'),
('Кудрявцева Ева Бульба','89535078985','login2','pass2','Мастер'),
('Гончарова Ульяна Бульба','89210673849','login3','pass3','Мастер'),
('Гусева Виктория Бульба','89990563748','login4','pass4','Оператор'),
('Баранов Артём Бульба','89994563847','login5','pass5','Оператор'),
('Беспалова Екатерина Бульба','89219567844','login10','pass10','Мастер');

INSERT INTO Client 
VALUES
('Овчинников Фёдор Никитич','89219567849','login16','pass16'),
('Петров Никита Артёмович','89219567841','login17','pass17'),
('Ковалева Софья Владимировна','89219567842','login18','pass18'),
('Кузнецов Сергей Матвеевич','89219567843','login19','pass19');

INSERT INTO Comments
Values
('Всё сделаем!', 2, 3),
('Всё сделаем!', 3, 2),
('Починим в момент!', 3, 1);

INSERT INTO RequestStatus
Values
('Выполнено'),
('В процессе'),
('В ожидании');

INSERT INTO Technic
VALUES
('Кондиционер', 'TCL TAC-12CHSA/TPG-W белый'),
('Кондиционер', 'Electrolux EACS/I-09HAT/N3_21Y белый'),
('Увлажнитель воздуха', 'Xiaomi Smart Humidifier 2'),
('Увлажнитель воздуха', 'Polaris PUH 2300 WIFI IQ Home'),
('Сушилка для рук', 'Ballu BAHD-1250');

INSERT INTO Requests
VALUES
('2023-06-06',1,'Не охлаждает воздух',2,null,'',2,1),
('2023-05-05',2,'Выключается сам по себе',2,null,'',3,1),
('2023-07-07',3,'Пар имеет неприятный запах',1,null,'',3,2),
('2023-08-02',4,'Увлажнитель воздуха продолжает работать при предельном снижении уровня воды',3,'2023-12-02','',null,3),
('2023-08-02',5,'Не работает',3,null,'',null,4);
