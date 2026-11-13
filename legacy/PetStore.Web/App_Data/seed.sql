-- seed data. DO NOT put semicolons inside text values, the loader splits on them!!
-- extra tables that EF doesn't know about (used by Utils.Log and Utils.GetSetting)
CREATE TABLE IF NOT EXISTS AuditLog (Id INTEGER PRIMARY KEY AUTOINCREMENT, Message TEXT, Who TEXT, At TEXT);
CREATE TABLE IF NOT EXISTS Settings ([Key] TEXT PRIMARY KEY, [Value] TEXT);

INSERT INTO Settings ([Key],[Value]) VALUES ('TaxRate','0.08');
INSERT INTO Settings ([Key],[Value]) VALUES ('ShippingFee','9.95');
INSERT INTO Settings ([Key],[Value]) VALUES ('MaintenanceMode','false');
INSERT INTO Settings ([Key],[Value]) VALUES ('AdminEmail','admin@petstore.local');

INSERT INTO Categories (Id, Name, Description, SortOrder) VALUES (1,'Dogs','Loyal friends of all sizes',1);
INSERT INTO Categories (Id, Name, Description, SortOrder) VALUES (2,'Cats','Independent and cuddly',2);
INSERT INTO Categories (Id, Name, Description, SortOrder) VALUES (3,'Birds','Colourful and chatty',3);
INSERT INTO Categories (Id, Name, Description, SortOrder) VALUES (4,'Fish','Calm aquarium companions',4);
INSERT INTO Categories (Id, Name, Description, SortOrder) VALUES (5,'Reptiles','Scaly and fascinating',5);

INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (1,'Buddy',1,'Golden Retriever','3 months',1200.00,'<b>Friendly</b> golden pup, great with kids.','/images/pets/pet-1.svg',0,1,'02/01/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (2,'Max',1,'German Shepherd','4 months',1350.00,'Smart and <i>very</i> loyal.','/images/pets/pet-2.svg',0,0,'15/02/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (3,'Bella',1,'Beagle','2 yrs',650.50,'Loves walks and snacks.','/images/pets/pet-3.svg',0,1,'03/11/2026');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (4,'Rocky',1,'Bulldog','1.5 years',1800.00,'Chill couch potato.','/images/pets/pet-4.svg',1,0,'20/03/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (5,'Daisy',1,'Poodle','6 months',999.99,'Hypoallergenic and <b>clever</b>.','/images/pets/pet-5.svg',0,0,'11/04/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (6,'Luna',2,'Siamese','5 months',450.00,'Talkative and affectionate.','/images/pets/pet-6.svg',0,1,'01/05/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (7,'Oliver',2,'Maine Coon','1 yr',800.00,'Gentle giant.','/images/pets/pet-7.svg',0,0,'09/11/2026');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (8,'Chloe',2,'Persian','8 months',720.25,'Fluffy and calm.','/images/pets/pet-8.svg',0,0,'22/06/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (9,'Simba',2,'Bengal','3 months',1100.00,'Energetic explorer.','/images/pets/pet-9.svg',0,0,'30/06/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (10,'Kiwi',3,'Budgerigar','6 months',35.10,'Sings every morning.','/images/pets/pet-10.svg',0,1,'05/07/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (11,'Polly',3,'African Grey','2 yrs',2500.00,'Can say <b>hello</b> and <b>pretty bird</b>.','/images/pets/pet-11.svg',0,0,'14/07/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (12,'Sunny',3,'Cockatiel','1 yr',150.00,'Whistles tunes.','/images/pets/pet-12.svg',0,0,'01/08/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (13,'Blue',3,'Lovebird','9 months',89.90,'Comes with a friend (sold separately).','/images/pets/pet-13.svg',0,0,'12/08/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (14,'Nemo',4,'Clownfish','4 months',24.99,'Bright orange reef fish.','/images/pets/pet-14.svg',0,1,'20/08/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (15,'Goldie',4,'Goldfish','2 months',4.99,'Classic bowl buddy.','/images/pets/pet-15.svg',0,0,'25/08/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (16,'Bubbles',4,'Betta','5 months',14.30,'Flowing fins.','/images/pets/pet-16.svg',0,0,'02/09/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (17,'Dory',4,'Blue Tang','6 months',59.95,'Forgetful but fun.','/images/pets/pet-17.svg',1,0,'10/09/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (18,'Spike',5,'Bearded Dragon','1 yr',250.00,'Docile and curious.','/images/pets/pet-18.svg',0,1,'18/09/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (19,'Shelly',5,'Box Turtle','5 yrs',180.00,'Will outlive us all.','/images/pets/pet-19.svg',0,0,'01/10/2024');
INSERT INTO Pets (Id,Name,CategoryId,Breed,Age,Price,Description,ImageUrl,IsSold,IsFeatured,CreatedDate) VALUES (20,'Kaa',5,'Ball Python','2 yrs',320.70,'Shy and gentle. <u>Feeding guide included</u>.','/images/pets/pet-20.svg',0,0,'12/11/2026');

INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (1,'Premium Dog Food 10kg',1,'59.99',25,'Grain free adult formula','/images/supplies/supply-1.svg','WoofCo',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (2,'Chew Toy Bone',1,'7.49',100,'Durable rubber bone','/images/supplies/supply-2.svg','WoofCo',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (3,'Dog Leash 2m',1,'19.95',3,'Reflective nylon leash','/images/supplies/supply-3.svg','PawsUp',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (4,'Cat Litter 15L',2,'22.10',40,'Clumping clay litter','/images/supplies/supply-4.svg','Meowz',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (5,'Scratching Post',2,'45.00',8,'Sisal rope post 80cm','/images/supplies/supply-5.svg','Meowz',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (6,'Catnip Mouse',2,'3.33',0,'Out of stock until supplier fixes things','/images/supplies/supply-6.svg','Meowz',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (7,'Bird Seed Mix 2kg',3,'12.99',60,'Mixed seeds for small birds','/images/supplies/supply-7.svg','Tweety',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (8,'Bird Cage Large',3,'149.00',2,'Powder coated steel cage','/images/supplies/supply-8.svg','Tweety',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (9,'Cuttlebone',3,'2.20',200,'Calcium supplement','/images/supplies/supply-9.svg','Tweety',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (10,'Aquarium 60L',4,'129.90',5,'Glass tank with lid','/images/supplies/supply-10.svg','AquaPro',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (11,'Fish Flakes 100g',4,'6.10',80,'Tropical fish flakes','/images/supplies/supply-11.svg','AquaPro',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (12,'Water Conditioner',4,'9.99',30,'Removes chlorine','/images/supplies/supply-12.svg','AquaPro',2);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (13,'Heat Lamp 100W',5,'34.50',12,'Basking lamp','/images/supplies/supply-13.svg','ScaleCare',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (14,'Terrarium 90cm',5,'219.00',1,'Front opening terrarium','/images/supplies/supply-14.svg','ScaleCare',1);
INSERT INTO Supplies (SupplyId,SupplyName,CatId,Price,Stock,Desc,ImagePath,Brand,Active) VALUES (15,'Calcium Powder',5,'11.11',45,'Dusting powder for insects','/images/supplies/supply-15.svg','ScaleCare',1);

-- passwords: admin = admin123, jdoe = password, msmith = password
INSERT INTO Users (Id,Username,PasswordHash,Email,FullName,Address,City,Zip,IsAdmin,CreatedAt) VALUES (1,'admin','0192023a7bbd73250516f069df18b500','admin@petstore.local','Site Admin','1 Admin St','Melbourne','3000',1,'2015-06-01 09:00:00');
INSERT INTO Users (Id,Username,PasswordHash,Email,FullName,Address,City,Zip,IsAdmin,CreatedAt) VALUES (2,'jdoe','5f4dcc3b5aa765d61d8327deb882cf99','jdoe@example.com','John Doe','12 Bark Rd','Sydney','2000',0,'2016-03-12 10:30:00');
INSERT INTO Users (Id,Username,PasswordHash,Email,FullName,Address,City,Zip,IsAdmin,CreatedAt) VALUES (3,'msmith','5f4dcc3b5aa765d61d8327deb882cf99','msmith@example.com','Mary Smith','7 Purr Ave','Brisbane','4000',0,'2018-11-02 14:15:00');
