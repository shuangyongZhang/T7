--记录数据库版本 升级表需要
DROP TABLE DB_VERSION;
CREATE TABLE DB_VERSION
(
    ID INTEGER PRIMARY KEY,
    Version VARCHAR(6)
);
INSERT INTO DB_VERSION(ID,Version) VALUES ( 1, '1.0');

--医院信息
DROP TABLE HospitalInfo;
CREATE TABLE HospitalInfo(
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Code VARCHAR(32),
    Name VARCHAR(32),
    Dept VARCHAR(32),
    Address VARCHAR(32),
    Desc VARCHAR(32),
    Telephone VARCHAR(16),
    ModifyTime DATE,
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);
INSERT INTO HospitalInfo(ID,Code,Name,Dept,Address,Desc,Telephone) VALUES ( 1, '', '南京康复医院', '康复科', '', '', '');
--管理员
DROP TABLE AdminInfo;
CREATE TABLE AdminInfo(
    Account VARCHAR(32) PRIMARY KEY NOT NULL,
    Password VARCHAR(32) NOT NULL,
    Role SMALLINT NOT NULL,
    Sex SMALLINT,
    Dept VARCHAR(32),
    Name VARCHAR(32),
    Telephone VARCHAR(16),
    IsForbidden Boolean NOT NULL,
    Creator VARCHAR(32) NOT NULL,
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);

update AdminInfo set Role=1 where Account= "1";
-- SELECT * FROM AdminInfo;
--INSERT INTO AdminInfo(Account,Password,Dept,Telephone,Role) VALUES ( 'admin', '1234', "康复部",'13564984964',1);
--update AdminInfo set CreateTime=datetime('now','localtime') where Account= "admin";
--患者详情
DROP TABLE PatientInfo;
CREATE TABLE PatientInfo(
    ID VARCHAR(32) PRIMARY KEY NOT NULL,
    Name VARCHAR(16) NOT NULL,
    Sex SMALLINT NOT NULL,
    Age SMALLINT NOT NULL,
    AffectedSide SMALLINT NOT NULL,
    Stature SMALLINT NOT NULL,
    Weight SMALLINT NOT NULL,
    CardNum VARCHAR(32),
    Doctor VARCHAR(16) NOT NULL,
    Diagnose VARCHAR(256),
    AdmissionDate DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);
-- SELECT * FROM PatientInfo;
--BirthDate DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
--ModifyTime DATE NOT NULL DEFAULT (datetime('now','localtime')),

--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0001','张三', 1, '罗翔', '',  '12',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0002','卖瓜的小贩', 0,  '祥子', '',  '122',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0003','陈五', 1, '罗翔', '',  '63',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0004','李二', 1,  '罗翔', '',  '34',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0005','王六', 0,  '罗翔', '',  '45',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0006','刘大', 1,  '罗翔', '',  '58',  '2021/11/1');
--INSERT INTO PatientInfo(ID,Name,Sex,Doctor,Diagnose,Age,AdmissionDate) VALUES ( '0008','许仙', 1,  '法海', '',  '96',  '2021/11/1');

--患者报告
DROP TABLE PatientReport;
CREATE TABLE PatientReport(
    ReportID INTEGER PRIMARY KEY AUTOINCREMENT,
    ReportName VARCHAR(32) NOT NULL,
    PatientID VARCHAR(32) NOT NULL,
    ReportType SMALLINT NOT NULL,
    TrainPart SMALLINT NOT NULL,
    TrainParam VARCHAR(256) NOT NULL,
    UpperData BLOB,
    LowerData BLOB,
    DoctorAdvice VARCHAR(256),
    StartTime DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
    EndTime DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
);


-- DROP TABLE TrainPresetSetting;
-- CREATE TABLE TrainPresetSetting(
--     Name VARCHAR(32) PRIMARY KEY NOT NULL,
--     Part SMALLINT NOT NULL,
--     Direction SMALLINT NOT NULL,
--     EnduranceMode SMALLINT NOT NULL,
--     TotalTime SMALLINT NOT NULL,
--     TotalRotations SMALLINT NOT NULL,
--     DurationTime SMALLINT NOT NULL,
--     RestTime SMALLINT NOT NULL,
--     AutoReverse SMALLINT NOT NULL,
--     TargetSpeed SMALLINT NOT NULL,
--     PassiveSpeed SMALLINT NOT NULL,
--     AssistSpeed SMALLINT NOT NULL,
--     AssistLevel SMALLINT NOT NULL,
--     ProgressiveResistance SMALLINT NOT NULL,
--     ProgressiveInterval SMALLINT NOT NULL,
--     Resistance SMALLINT NOT NULL,
--     MinResistance SMALLINT NOT NULL,
--     MaxResistance SMALLINT NOT NULL,
--     CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime'))
-- );

DROP TABLE GameHistory;
CREATE TABLE GameHistory(
    GameID INTEGER PRIMARY KEY NOT NULL,
    Level VARCHAR(32) NOT NULL,
    Time VARCHAR(32) NOT NULL,
    Score VARCHAR(32) NOT NULL
);

DROP TABLE PrescriptionHistory;
CREATE TABLE PrescriptionHistory(
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(32) NOT NULL,
    CreateTime DATE NOT NULL DEFAULT (datetime('now', 'localtime')),
    GameTypesStr VARCHAR(256) NOT NULL,
    TrainSettingStr VARCHAR(256) NOT NULL
);

DROP TABLE GameTopPlayer;
CREATE TABLE GameTopPlayer(
    GameID INTEGER PRIMARY KEY NOT NULL,
    RankInfoStr VARCHAR(512) NOT NULL
);
